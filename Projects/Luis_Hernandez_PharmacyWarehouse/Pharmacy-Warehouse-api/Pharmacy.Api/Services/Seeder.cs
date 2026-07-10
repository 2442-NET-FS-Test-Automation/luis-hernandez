using Pharmacy.Data.Configurations;
using Pharmacy.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Pharmacy.Data;
using Serilog;
using Pharmacy.Api.DTO;
using Pharmacy.Api.Services;

/// <summary>
/// Genera órdenes de prueba reutilizando Customers y Products ya sembrados
/// (ver PharmacyDbContext.OnModelCreating -> Data Seeding). Pensado para
/// demostrar el modelo de workers/cola: crea N órdenes Pending y deja que
/// /orders/burst las encole en PriorityOrderQueue.
/// </summary>

public interface ISeeder
{
    IReadOnlyCollection<SeededOrder> SeedOrders(int n);
    public IReadOnlyCollection<SeededOrder> ResetAndCreateOrders(int n);
}

public class Seeder : ISeeder
{
    // Going ahead and hardcoding some item SKUs (barcode numbers essentially in a list)
    private static readonly string[] Skus = { "PZ-1001", "PZ-1002, SF-2001" };

    private readonly IDbContextFactory<PharmacyDbContext> _factory;

    private readonly IFulfillmentService _fs;

    public Seeder(IDbContextFactory<PharmacyDbContext> factory, IFulfillmentService fulfillmentService)
    {
        _factory = factory;
        _fs = fulfillmentService;
    }

    public IReadOnlyCollection<SeededOrder> ResetAndCreateOrders(int n)
    {
        using var db = _factory.CreateDbContext();

        foreach (InventoryItem inv in db.Inventory)
        {
            switch (inv.ProductId)
            {
                case 1: inv.CurrentStock = 120; break;
                case 2: inv.CurrentStock = 8; break;
                case 3: inv.CurrentStock = 45; break;
                default: break;
            }
        }

        db.SaveChanges(); // saving changes after reset

        return SeedOrders(n);
    }

    public IReadOnlyCollection<SeededOrder> SeedOrders(int n)
    {
        using var db = _factory.CreateDbContext();

        var customerIds = db.Customers.Select(c => c.Id).ToList();
        var productIds = db.Products.Select(p => p.Id).ToList();

        if (customerIds.Count == 0 || productIds.Count == 0)
        {
            throw new InvalidOperationException(
                "Se necesita al menos un Customer y un Product en la base de datos para generar órdenes de prueba.");
        }

        var random = Random.Shared;
        var orders = new List<Order>();

        for (var i = 0; i < n; i++)
        {
            // Round-robin sobre los Customers existentes; producto y cantidad
            // aleatorios para que algunas órdenes agoten stock a propósito
            // (útil para ver Backordered en la demo, no solo Fulfilled).
            var customerId = customerIds[i % customerIds.Count];
            var productId = productIds[random.Next(productIds.Count)];
            var units = random.Next(1, 6);

            // 1. Procesamos las líneas primero
            var orderLines = new List<OrderLine>
                {
                    new() { ProductId = productId, Units = units }
                };

            // 2. Calculamos el total usando las líneas ya creadas
            var totalPrice = orderLines.Sum(line => line.Units * _fs.GetProductPrice(line.ProductId));

            orders.Add(new Order
            {
                CustomerId = customerId,
                Priority = i % 3 == 0 ? OrderPriority.Expedited : OrderPriority.Normal,
                Status = OrderStatus.Pending,
                Lines = new List<OrderLine>
                {
                    new() { ProductId = productId, Units = units }
                },
                CreatedUtc = DateTime.UtcNow,
                TotalPrice = totalPrice
            });
        }

        db.Orders.AddRange(orders);
        db.SaveChanges();

        Log.Information("Seeded {Count} test orders Normal: {NormalPriority}, Expedited: {ExpeditedPriority}",
        orders.Count,
        orders.Count(o => o.Priority == OrderPriority.Normal),
        orders.Count(o => o.Priority == OrderPriority.Expedited)
        );

        return orders.Select(o => new SeededOrder (o.Id, o.Priority) ).ToList();
    }
}