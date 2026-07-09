using Microsoft.EntityFrameworkCore;
using Pharmacy.Api.Services;
using Pharmacy.Data;
using Pharmacy.Data.Configurations;

using Serilog;


namespace Pharmacy.Api.Queueing;

/// <summary>
/// Al arrancar, crea un "worker loop" por cada Dispatcher existente en la
/// base de datos. Cada loop representa a ese Dispatcher jalando (pull)
/// órdenes de PriorityOrderQueue cuando está libre: se marca Busy, procesa
/// vía IFulfillmentService, y se libera (Free) al terminar.
///
/// Es Singleton (como todo BackgroundService); resuelve sus dependencias
/// Scoped (PharmacyDbContext, IFulfillmentService) creando un IServiceScope
/// nuevo por cada orden procesada.
/// </summary>
public class DispatcherWorkerService : BackgroundService
{
    private readonly PriorityOrderQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;

    

    public DispatcherWorkerService(
        PriorityOrderQueue queue,
        IServiceScopeFactory scopeFactory)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<int> dispatcherIds;

        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();
            dispatcherIds = await db.Dispatchers.Select(d => d.Id).ToListAsync(stoppingToken);
        }

        if (dispatcherIds.Count == 0)
        {
            Log.Warning("There isn't registered Dispatchers; no order will be processed.");
            return;
        }

        Log.Information("Starting {Count} worker loops of Dispatcher: [{Ids}]", dispatcherIds.Count, string.Join(", ", dispatcherIds));

        var workerLoops = dispatcherIds.Select(id => RunDispatcherLoopAsync(id, stoppingToken));
        await Task.WhenAll(workerLoops);
    }

    private async Task RunDispatcherLoopAsync(int dispatcherId, CancellationToken ct)
    {
        Log.Information("Dispatcher {DispatcherId} worker loop started", dispatcherId);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var orderId = await _queue.DequeueAsync(ct);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var provider = scope.ServiceProvider;
                var db = provider.GetRequiredService<PharmacyDbContext>();
                var fulfillmentService = provider.GetRequiredService<IFulfillmentService>();

                if (!await DispatcherAllocation.TryMarkBusyAsync(db, dispatcherId, ct))
                {
                    // El dispatcher ya estaba Busy por otra vía (poco común dado
                    // que este loop es el único dueño de este id); reencolamos
                    // la orden para que no se pierda y seguimos.
                    _queue.Enqueue(orderId, OrderPriority.Normal);
                    continue;
                }

                try
                {
                    // Simula el tiempo real que le toma a un dispatcher preparar
                    // un pedido (empacar, verificar, etc.). Sin esto, en local
                    // las operaciones son tan rápidas (sub-milisegundo) que un
                    // solo worker gana casi siempre la carrera por el siguiente
                    // item antes de que el otro llegue a competir por turno —
                    // no es un bug del modelo, es que no hay trabajo real que
                    // simule ocupación. Ajusta o quita este delay cuando
                    // conectes trabajo real (ej. llamadas a un WMS externo).
                    await Task.Delay(30, ct);

                    var result = await fulfillmentService.FulfillOneAsync(orderId, dispatcherId, ct);

                    _queue.CompleteOrder(orderId, result);
                } catch(Exception ex)
                {
                    _queue.FaultOrder(orderId, ex);
                    throw;
                }
                finally
                {
                    await DispatcherAllocation.MarkFreeAsync(db, dispatcherId, ct);
                }
            }
            catch (OperationCanceledException)
            {
                break; // apagado gracioso del host
            }
            catch (Exception ex)
            {
                // Antes, una excepción fuera de la llamada a FulfillOneAsync
                // (ej. en TryMarkBusyAsync/MarkFreeAsync) escapaba sin capturar
                // y mataba este loop para siempre — el Dispatcher dejaba de
                // trabajar sin ningún log que lo explicara. Ahora todo el
                // cuerpo del loop está cubierto: se registra el error y el
                // worker sigue vivo para la próxima orden.
                Log.Error(ex, "Dispatcher {DispatcherId} worker loop error", dispatcherId);
            }
        }

        Log.Information("Dispatcher {DispatcherId} worker loop stopped", dispatcherId);
    }
}