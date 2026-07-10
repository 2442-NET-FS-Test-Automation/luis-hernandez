using Microsoft.EntityFrameworkCore;
using Pharmacy.Data.Configurations;
using Serilog;
using Pharmacy.Api.Extensions;
using Pharmacy.Data.Entities;
using Pharmacy.Api.DTO;
using Pharmacy.Api.Services;
using Pharmacy.Data.Exceptions;
using Pharmacy.Api.Queueing;
using Pharmacy.Data;
using System.Diagnostics;

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/fulfillment-log-.log",
        rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Pharmacy Warehouse API");

    var builder = WebApplication.CreateBuilder(args);
    // ---------- BUILDER: SERVICES REGISTRY ----------

    // Replace the default logger provider with Serilog
    // ReadFrom.Configuration takes "Serilog" section from appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Persistency
    // PharmacyDbContext (Scoped) + IDbContextFactory(Singleton) 
    // For concurrent operations
    builder.Services.AddPersistence(builder.Configuration);

    // Application Services
    builder.Services.AddApplicationServices();

    var app = builder.Build();

    // ---------- Pipeline: middlewares and endpoints ----------

    //Swagger conf for dev env
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    //--------- Reports endpoints ------------

    // Get complete inventory
    app.MapGet("/reports/inventory", async (PharmacyDbContext db) =>
    {
        return await db.Inventory.ToListAsync();
    });

    // Query using LINQ: grouping inventory by stock
    app.MapGet("/reports/inventory/by-value", async (PharmacyDbContext db) =>
    {
        return await db.Inventory
            .Include(i => i.Product)
            .GroupBy(i => i.CurrentStock >= 5
                ? "well-stocked"
                : "low")
            .Select(g => new
            {
                tier = g.Key,
                count = g.Count(),
                units = g.Sum(i => i.CurrentStock)
            })
            .ToListAsync();
    });

    app.MapGet("/reports/orders/by-completion", (PharmacyDbContext db) =>
    {
        return db.Orders // look inside orders table
             .Where(o => o.Status == OrderStatus.Fulfilled) // grab fulfilled orders
             .OrderBy(o => o.CompletedUtc) // order by when they were completed
             .Select(o => new { o.Id, o.Priority, o.CompletedUtc }) // use info from those orders to make some return objects
             .ToList(); // put them in a list and return them as JSON body of response
    });

    app.MapGet("/reports/products/top-products", (PharmacyDbContext db) =>
    {
        var ranked = db.FulfillmentEvents
            .Where(e => e.Type == "Fulfilled")
            .Join(db.OrderLines, e => e.OrderId, l => l.OrderId, (e, l) => l)
            .GroupBy(l => l.ProductId)
            .Select(g => new { ProductId = g.Key, Units = g.Sum(l => l.Units) })
            .OrderByDescending(x => x.Units)
            .ToList();

        return ranked;
    });

    // Binary search on the sorted result
    app.MapGet("/reports/rank-of/{units:int}", (int units, PharmacyDbContext db) =>
    {
        // Find product ranking that sold x units 
        var unitsDesc = db.FulfillmentEvents
            .Where(e => e.Type == "Fulfilled")
            .Join(db.OrderLines, e => e.OrderId, l => l.OrderId, (e, l) => l)
            .GroupBy(l => l.ProductId)
            .Select(g => g.Sum(l => l.Units))
            .OrderByDescending(u => u)
            .ToArray();


        // sorted DESC => using Binary Search to find the index of a specific quantity sold ex 1000, 400, 330, 34
        // Our BinarySearch needs a comparer - for something like an int or a char 
        var index = Array.BinarySearch(unitsDesc, units, Comparer<int>.Create((a, b) => b.CompareTo(a)));
        return new { units, rank = index >= 0 ? index + 1 : -1 }; 
        // If BinarySearch doesn't find a thing - it returns some bitwise 
        // complement or something - we collapse it to -1 
    });


    // Util for demo day
    app.MapPost("/inventory/reset", (PharmacyDbContext db) =>
    {
        Log.Information("Starting inventory reset");

        foreach (InventoryItem inv in db.Inventory)
        {
            switch (inv.Id)
            {
                case 1:
                    inv.CurrentStock = 120;
                    break;

                case 2:
                    inv.CurrentStock = 8;
                    break;

                case 3:
                    inv.CurrentStock = 45;
                    break;
            }
        }

        db.SaveChanges();

        Log.Information(
            "Inventory restored correctly");

        return Results.Ok("stock reset");
    });

    // -------- Order endpoints -----------

    // Create a new order: it is saved as Pending and is enqueued for
    // being processed on background by the DispatcherWorker's
    // (watch DispatcherWorkerService.cs). Response 201 immediately,
    // without hooping for fulfilling that order.
    app.MapPost("/orders/request", async (
        OrderRequest req,
        OrderFactory factory,
        PriorityOrderQueue queue,
        IDbContextFactory<PharmacyDbContext> dbf,
        CancellationToken ct) =>
    {
        try
        {
            Order newOrder = factory.CreateOrder(
                req.Kind,
                req.CustomerId,
                req.OrderLines.Select(l => (l.Batch, l.Qty)));

            await using var db = await dbf.CreateDbContextAsync(ct);
            db.Orders.Add(newOrder);
            await db.SaveChangesAsync(ct);

            queue.Enqueue(newOrder.Id, newOrder.Priority);

            Log.Information("Order {OrderId} created and enqueued ({Priority})", newOrder.Id, newOrder.Priority);

            return Results.Created($"/orders/{newOrder.Id}", new { newOrder.Id, newOrder.Status });
        }
        catch (UnknownBatchException ex)
        {
            Log.Warning("Rejected order: unknown batch {Batch}", ex.Batch);
            return Results.BadRequest(new { error = ex.Message });
        }
    });

    // Generes 'n' test orders and then enqueue them into PriorityOrderQueue, works
    // like /orders/request but with burst requests (DispatcherWorkerService running 
    // in background is gonna take them)
    // This endpoint is useful for showing:
    // workers model: logs will show how distinct Dispatchers are marked as
    // Busy/Free according as they goes pulling orders from the queue.
    app.MapPost("/orders/burst", (int n, ISeeder seeder, PriorityOrderQueue queue) =>
    {
        var orders = seeder.SeedOrders(n);

        foreach (var order in orders)
        {
            queue.Enqueue(order.Id, order.Priority);
        }

        Log.Information("Burst: {Count} orders seeded and enqueued", orders.Count);

        return Results.Ok(new { enqueued = orders.Count, orderIds = orders.Select(o => o.Id) });
    });

    //--------------- Test endpoints ----------------
    
    //Grab Inventory Items and count those that have negative inventory
    app.MapGet("/test/no-oversell", (PharmacyDbContext db) =>
    {
        var rows = db.Inventory.Include(i => i.Product).ToList(); // grab Inventory rows, include the product objects as well
        var negative = rows.Where(i => i.CurrentStock < 0).ToList(); //grab items with negative stock
        var fulfilled = db.FulfillmentEvents.Count(e => e.Type == "Fulfilled"); // count the fulfilled orders

        return new
        {
            anyNegative = negative.Any(),
            onHand = rows.Select(i => new { i.ProductId, i.CurrentStock }),
            unitsFulfilled = fulfilled
        };
    });

    //Tests how sequential vs concurrent/Parallel runs compare - with mixed orders
    app.MapPost("/test/benchmark", async (int n, IFulfillmentService fs, ISeeder seeder, PriorityOrderQueue queue, CancellationToken ct) =>
    {
        // Lets see how sequential vs concurrent/parallel runs compare - with mixed orders
        var sequentialOrders = seeder.ResetAndCreateOrders(n);

        // First, sequential
        var sw1 = Stopwatch.StartNew(); // start our stopwatch

        foreach (var order in sequentialOrders)

            await fs.FulfillWithAnyAvailableDispatcherAsync(order.Id, ct);

        sw1.Stop();

        // Next concurrent
        var orders = seeder.ResetAndCreateOrders(n);

        var sw2 = Stopwatch.StartNew(); // start second stopwatch

        await fs.FulfillBurstAsync(orders.Select(o => o.Id), ct);

        sw2.Stop();

        return new
        {
            sequentialMs = sw1.ElapsedMilliseconds,
            concurrentMs = sw2.ElapsedMilliseconds,
            difference = sw2.ElapsedMilliseconds - sw1.ElapsedMilliseconds,
        };

    });

    app.Run();
}
catch (Exception e)
{
    Log.Fatal("The application terminated unexpectedly during startup: \n Message: {Message}", e.Message);
}
finally
{
    Log.CloseAndFlush();
}