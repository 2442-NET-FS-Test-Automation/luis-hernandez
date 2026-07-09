using System.Collections.Concurrent;
using Pharmacy.Api.Services;
using Pharmacy.Data;

namespace Pharmacy.Api.Queueing;

/// <summary>
/// Cola de prioridad in-memory. Modelo "pull": los DispatcherWorker's llaman
/// a DequeueAsync cuando quedan libres, en vez de que el sistema empuje
/// trabajo hacia ellos. Vive como Singleton — es estado compartido por todo
/// el proceso.
///
/// Importante: es en memoria. Si el proceso se reinicia, las órdenes que
/// estaban encoladas pero no tomadas se pierden de la cola (aunque su
/// Status = Pending sigue intacto en la base de datos). Si necesitas
/// resiliencia ante reinicios, DispatcherWorkerService es el lugar natural
/// para reencolar órdenes Pending al arrancar.
/// </summary>
public class PriorityOrderQueue
{
    private readonly PriorityQueue<int, int> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly object _gate = new();

    private readonly ConcurrentDictionary<int, TaskCompletionSource<FulfillmentResult>> _pending = new();

    /// Enqueues an order. Expedited always goes out before Normal.
    public void Enqueue(int orderId, OrderPriority priority)
    {
        var rank = priority == OrderPriority.Expedited ? 0 : 1;

        lock (_gate)
        {
            _queue.Enqueue(orderId, rank);
        }

        _signal.Release();
    }

    public Task<FulfillmentResult> EnqueueAndWaitAsync(int orderId, OrderPriority priority)
    {
        var tcs = _pending.GetOrAdd(orderId,
            _ => new TaskCompletionSource<FulfillmentResult>(TaskCreationOptions.RunContinuationsAsynchronously));

        Enqueue(orderId, priority);

        return tcs.Task;
    }


    /// Waits asynchronously until an order is available and the 
    /// returns (highest priority first). It is the "pull" operation 
    /// used by the DispatcherWorker's.
    public async Task<int> DequeueAsync(CancellationToken ct)
    {
        await _signal.WaitAsync(ct);

        lock (_gate)
        {
            _queue.TryDequeue(out var orderId, out _);
            return orderId;
        }
    }

    public void CompleteOrder(int orderId, FulfillmentResult result)
    {
        if (_pending.TryRemove(orderId, out var tcs))
        {
            tcs.TrySetResult(result);
        }
    }

    public void FaultOrder(int orderId, Exception ex)
    {
        if (_pending.TryRemove(orderId, out var tcs))
        {
            tcs.TrySetException(ex);
        }
    }
}