using System.Collections.Concurrent;
using Pharmacy.Api.Services;
using Pharmacy.Data;

namespace Pharmacy.Api.Queueing;

/// <summary>
/// In-memory priority queue. Uses a "pull" model: DispatcherWorkers call
/// DequeueAsync when they become available, rather than the system pushing
/// work to them. It operates as a Singleton—its state is shared across
/// the entire process.
///
/// Important: it is in-memory. If the process restarts, orders that
/// were queued but not yet picked up are lost from the queue.
/// However, their Status remains "Pending" in the database; this fact
/// can be leveraged to implement resilience against restarts by
/// re-queueing "Pending" orders upon startup.
/// </summary>
public class PriorityOrderQueue
{
    private readonly PriorityQueue<int, int> _queue = new();

    // A semaphore set to 0. Since it is set to zero, no one can proceed past it; it functions like an automated red light.
    private readonly SemaphoreSlim _signal = new(0);
    //An empty object that serves only as a "lock" to ensure that two threads do not modify the queue at the same time.
    private readonly object _gate = new();
    //A dictionary that will store orders currently being delivered, in order to provide notification when they are completed.
    private readonly ConcurrentDictionary<int, TaskCompletionSource<FulfillmentResult>> _pending = new();

    /// Enqueues an order. Expedited always goes out before Normal.
    public void Enqueue(int orderId, OrderPriority priority)
    {

        var rank = priority == OrderPriority.Expedited ? 0 : 1;

        // The lock is activated using `lock (_gate)`. This ensures that, if 100 
        // orders were to arrive simultaneously, the internal structure of `_queue` 
        // organizes them one by one without corrupting memory. The order is stored 
        // in the queue according to its priority.
        lock (_gate)
        {
            _queue.Enqueue(orderId, rank);
        }

        //_signal.Release(); is executed. This increments the semaphore counter from 0 to 1.
        _signal.Release();
    }

    // When the client enqueues an order that requires notification upon completion, the system calls EnqueueAndWaitAsync.
    public Task<FulfillmentResult> EnqueueAndWaitAsync(int orderId, OrderPriority priority)
    {
        // The code registers the order in the _pending dictionary along with a 
        // TaskCompletionSource (TCS)
        // The TCS serves to notify you when your order is ready.
        var tcs = _pending.GetOrAdd(orderId,
            _ => new TaskCompletionSource<FulfillmentResult>(TaskCreationOptions.RunContinuationsAsynchronously));

        // Internally calls the Enqueue method
        Enqueue(orderId, priority);

        // The method returns tcs.Task. The client thread waits (without blocking) for that task to complete in the future.
        return tcs.Task;
    }


    /// Waits asynchronously until an order is available and the 
    /// returns (highest priority first). It is the "pull" operation 
    /// used by the DispatcherWorker's.
    public async Task<int> DequeueAsync(CancellationToken ct)
    {
        //Many workers can call dequeeAsync at the same time 
        //but stop here at wait _signal.WaitAsunc()

        //Since the semaphore is at 0, the light is red. All the workers remain 
        // paused at that line. They consume neither memory nor CPU.
        await _signal.WaitAsync(ct);
        // ^
        // |
        // Immediately after the semaphore rises to 1
        // Lets exactly one worker pass and automatically lowers the semaphore 
        // counter back to 0

        lock (_gate)
        {
            // The priority queue will automatically extract the order with the lowest rank (0 before 1).
            _queue.TryDequeue(out var orderId, out _);
            return orderId;
        }
    }

    // When the worker successfully finishes processing the order, it calls CompleteOrder(orderId, result)
    public void CompleteOrder(int orderId, FulfillmentResult result)
    {
        //The system looks for the orderId in the _pending dictionary and removes it (TryRemove)
        if (_pending.TryRemove(orderId, out var tcs))
        {
            //Upon finding it, execute tcs.TrySetResult(result). This makes the "ticket vibrate."
            tcs.TrySetResult(result);

            //The client who had been waiting receives the FulfillmentResult and knows that their order has been executed.
        }
    }

    //If something had failed, the worker would call FaultOrder, which would send an exception/error to the client instead of a successful result.
    public void FaultOrder(int orderId, Exception ex)
    {
        if (_pending.TryRemove(orderId, out var tcs))
        {
            tcs.TrySetException(ex);
        }
    }
}