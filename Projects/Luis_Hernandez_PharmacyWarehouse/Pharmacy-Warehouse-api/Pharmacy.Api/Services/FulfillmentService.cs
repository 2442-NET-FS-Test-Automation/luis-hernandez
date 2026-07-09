using Pharmacy.Data.Configurations;
using Pharmacy.Data.Entities;
using Pharmacy.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Concurrent;

using Pharmacy.Api.Queueing;

namespace Pharmacy.Api.Services;


// Everything about order fulfillment process in this file
public interface IFulfillmentService
{
    Task<FulfillmentResult> FulfillOneAsync(int orderId, int dispatcherId, CancellationToken ct);
    Task<BurstResult> FulfillBurstAsync(IEnumerable<int> orderIds, CancellationToken ct);

    Task<FulfillmentResult> FulfillWithAnyAvailableDispatcherAsync(int orderId, CancellationToken ct);

    public int ResolveProductId(string sku);
}

public class FulfillmentService : IFulfillmentService
{
    private readonly IDbContextFactory<PharmacyDbContext> _factory;
    private readonly PriorityOrderQueue _queue;
    private readonly BurstPlanner _planner;
    private readonly ConcurrentDictionary<string, int> _batchToProductId;

    //Here I apply DI
    public FulfillmentService(IDbContextFactory<PharmacyDbContext> factory, BurstPlanner planner, PriorityOrderQueue queue)
    {
        _factory = factory;
        _queue = queue;
        _planner = planner;
        using var db = _factory.CreateDbContext();
        _batchToProductId = new ConcurrentDictionary<string, int>(
            db.Products.ToDictionary(p => p.Batch, p => p.Id)
        );
    }

    //Burst orders method
    //For shutdown gracefully
    //Here I just take on count the _planner for planning
    //Then the priority first order help to assign it to dispatchers
    //In fulfillOneAsync
    public async Task<BurstResult> FulfillBurstAsync(IEnumerable<int> orderIds, CancellationToken ct)
    {
        var idList = orderIds.ToList();

        List<Order> orders;
        await using (var db = await _factory.CreateDbContextAsync(ct))
        {
            orders = await db.Orders.Where(o => idList.Contains(o.Id)).ToListAsync(ct);
        }

        var planner = new BurstPlanner();
        var planned = planner.OrderByPriority(orders);

        var tasks = planned.Select(o => _queue.EnqueueAndWaitAsync(o.Id, o.Priority).WaitAsync(ct));
        var results = await Task.WhenAll(tasks);

        return new BurstResult(
            Fulfilled: results.Count(r => r == FulfillmentResult.Fulfilled),
            Backordered: results.Count(r => r == FulfillmentResult.Backordered));
    }
    
    public async Task<FulfillmentResult> FulfillOneAsync(int orderId, int dispatcherId, CancellationToken ct)
    {
        // Ask for a db context
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Look for a previous order placed in db
        // Order that now we are now fulfilling
        var order = await db.Orders
                    .Include(o => o.Lines)
                    .FirstAsync(o => o.Id == orderId, ct);

        // Dictionary k: productId v: Units
        var requested = order.Lines.ToDictionary(l => l.ProductId, l => l.Units);

        Log.Information("Performing order {OrderId} with priority {Priority}", order.Id, order.Priority);

        // This answer "can i continue fulfilling this order?"
        bool canFulfill = true;

        foreach (OrderLine line in order.Lines)
        {
            // Grab the current inventory for that product
            InventoryItem inv = await db.Inventory.FirstAsync(i => i.ProductId == line.ProductId, ct);

            // Next - check if we can meet the order 
            if (inv.CurrentStock < line.Units)
            {
                canFulfill = false;
                break;
            }

            // Write to the InventoryItem table where is guarded by RowVersion
            inv.CurrentStock -= line.Units;
        }

        // If we cannot fulfill the order
        if (!canFulfill)
        {
            // The this order is Backordered
            order.Status = OrderStatus.Backordered;

            // Create and save a new fulfillment event record for this transaction
            db.FulfillmentEvents.Add(new FulfillmentEvent { OrderId = orderId, DispatcherId = dispatcherId, Type = "Backorder" });

            await db.SaveChangesAsync(ct);

            Log.Warning("Backordered {OrderId}: insufficient stock", orderId);

            return FulfillmentResult.Backordered;
        }

        // If we are able to fulfill that order
        order.Status = OrderStatus.Fulfilled;
        order.CompletedUtc = DateTime.UtcNow;
        db.FulfillmentEvents.Add(new FulfillmentEvent
        {
            OrderId = orderId,
            DispatcherId = dispatcherId,
            Type = "Fulfilled"
        });

        // Retry save method for  concurrent writes to the same InventoryItem
        // if it's not possible, stock dropped in the meantime -> backordered.
        if (!await SaveWithRetryAsync(db, requested, ct))
        {
            // clear change tracker
            db.ChangeTracker.Clear();

            //grab stale order from db
            Order staleOrder = await db.Orders.FirstAsync(o => o.Id == orderId, ct);
            // set its status to backordered
            staleOrder.Status = OrderStatus.Backordered;
            await db.SaveChangesAsync(ct);

            Log.Warning("Backordered order {OrderId} after concurrency retry", orderId);
            return FulfillmentResult.Backordered;
        }

        Log.Information("Fulfilled order: {OrderId}, {LineCount} lines", orderId, order.Lines.Count);
        return FulfillmentResult.Fulfilled;
    }

    public async Task<FulfillmentResult> FulfillWithAnyAvailableDispatcherAsync(int orderId, CancellationToken ct)
    {
        var priority = await GetPriorityAsync(orderId, ct);
        return await _queue.EnqueueAndWaitAsync(orderId, priority).WaitAsync(ct);
    }

    public int ResolveProductId(string batch)
    {
        return _batchToProductId[batch];
    }



    // ------------ Util private methods ---------------------

    private async Task<OrderPriority> GetPriorityAsync(int orderId, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Orders.Where(o => o.Id == orderId).Select(o => o.Priority).FirstAsync(ct);
    }

    private static async Task<bool> SaveWithRetryAsync(
        PharmacyDbContext db,
        IReadOnlyDictionary<int, int> requestedByProductId,
        CancellationToken ct)
    {
        while (true)
        {
            try
            {
                // The DbContext here came from FulfillOneAsync
                // if it has changes staged to it - we can save them here. 
                // Its the same object.
                await db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Retry logic - using entry - an EF Core Change tracker entry
                foreach (var entry in ex.Entries)
                {
                    // grab the current database values
                    var current = await entry.GetDatabaseValuesAsync();

                    // Exit condition: the row no longer exist
                    if (current is null) return false;

                    entry.OriginalValues.SetValues(current);

                    if (entry.Entity is InventoryItem inv)
                    {
                        // Grab the current totals for that item's stock
                        int freshValue = current.GetValue<int>(nameof(InventoryItem.CurrentStock));

                        //Dictionary lookup against the dict we passed in
                        int desiredAmount = requestedByProductId[inv.ProductId];

                        // Exit condition: re-check against the fresh stock.
                        if (freshValue < desiredAmount) return false;

                        // Retry with refreshed value
                        inv.CurrentStock = freshValue - desiredAmount;
                    }
                }
            }
        }
    }
}

