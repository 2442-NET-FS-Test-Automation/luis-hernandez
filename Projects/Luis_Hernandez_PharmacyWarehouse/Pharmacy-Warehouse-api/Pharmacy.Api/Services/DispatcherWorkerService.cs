using Microsoft.EntityFrameworkCore;
using Pharmacy.Api.Services;
using Pharmacy.Data;
using Pharmacy.Data.Configurations;

using Serilog;


namespace Pharmacy.Api.Queueing;

/// <summary>
/// When starting, the program creates one "worker loop" per database Dispatcher 
/// Each loop can be seen as an Dispatcher pulling orders from PriorityOrderQueue
/// when is free: is marked as Busy, process the taken order using 
/// IFulfillmentService, an once finish the fulfillment process is marked as free.
/// when is busy: doesn't try to pull an order until is marked as free

/// Because is a BackgroundService it is Singleton
/// So we shouldn't inject services with life time Scoped (like DbContext) 
/// The class itself solves its Scoped dependencies ex. (PharmacyDbContext, IFulfillmentService) through creating a new IServiceScope per processed order 
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
                    // Dispatcher was previously busy (it should not occurs), 
                    // in this hypothetical case we repose the order on queue 
                    // so the order doesn't miss
                    _queue.Enqueue(orderId, OrderPriority.Normal);
                    continue;
                }

                // Here the system takes a dispatcher and assign him an order 
                // to be processed using fulfillmentService.FulfillOneAsync
                try
                {
                    
                    // Simulation - The delay mocks the time that takes to a 
                    // dispatcher packaging, verify, etc. and order
                    // Because - there isn't a real work performed (like an external WMS were dispatchers can handle its assigned orders)

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
                break; // host's gracefully shutdown
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Dispatcher {DispatcherId} worker loop error", dispatcherId);
            }
        }

        Log.Information("Dispatcher {DispatcherId} worker loop stopped", dispatcherId);
    }
}