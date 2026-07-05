using Microsoft.EntityFrameworkCore;
using Pharmacy.Data.Configurations;


var builder = WebApplication.CreateBuilder(args);

// ---------- BUILDER: SERVICES REGISTRY ----------

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Hardcoded connecton string to a database (I NEED TO MOVE IT)
var conn_string = "Server=localhost,1433;Database=PharmacyWarehouseDb;User Id=sa;Password=LibraryPass1!;TrustServerCertificate=true";

// Register DbContext
builder.Services.AddDbContext<PharmacyDbContext>(options => options.UseSqlServer(conn_string), ServiceLifetime.Scoped, ServiceLifetime.Singleton);

var app = builder.Build();

// ---------- Pipeline: middlewares and endpoints ----------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Simple endpoint for testing that its working
app.MapGet("/", () => "Hello World!");

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


app.Run();
