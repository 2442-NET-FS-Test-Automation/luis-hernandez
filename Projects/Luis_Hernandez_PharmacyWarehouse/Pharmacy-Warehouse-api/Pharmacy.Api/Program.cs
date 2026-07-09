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

    //--------- Inventory endpoints ------------

    // Get complete inventory
    app.MapGet("/inventory", async (PharmacyDbContext db) =>
    {
        return await db.Inventory.ToListAsync();
    });

    // Query using LINQ: grouping inventory by stock
    app.MapGet("/inventory/by-value", async (PharmacyDbContext db) =>
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

    // Crear una nueva orden: se guarda como Pending y se encola para
    // procesamiento en background por los DispatcherWorker's (ver
    // Queueing/DispatcherWorkerService.cs). Responde 201 de inmediato,
    // sin esperar a que la orden sea fulfilled.
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

    // Genera 'n' órdenes de prueba y las encola en PriorityOrderQueue, usando
    // la MISMA ruta de fulfillment que /orders/request (DispatcherWorkerService
    // ya corriendo en background las va a tomar). Sirve para demostrar el
    // modelo de workers: verás en los logs cómo distintos Dispatchers se
    // marcan Busy/Free a medida que van jalando órdenes de la cola.
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
    app.MapGet("/verify/no-oversell", (PharmacyDbContext db) =>
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

app.MapPost("/benchmark", async (int n, IFulfillmentService fs, ISeeder seeder, PriorityOrderQueue queue, CancellationToken ct) =>
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