using Microsoft.EntityFrameworkCore;
using Pharmacy.Data.Entities;
using Pharmacy.Data.Exceptions;

namespace Pharmacy.Data.Configurations;

public class PharmacyDbContext : DbContext
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options)
    { }

    //what C# classes we are tracking as Entities
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Laboratory> Laboratories => Set<Laboratory>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Dispatcher> Dispatchers => Set<Dispatcher>();
    public DbSet<FulfillmentEvent> FulfillmentEvents => Set<FulfillmentEvent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        //Here i can

        // Configure schema using Fluent API

        b.Entity<Laboratory>(e =>
        {
            e.Property(l => l.Name).HasMaxLength(100).IsRequired();
            e.Property(l => l.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(l => l.Email).IsUnique();
        });

        b.Entity<Product>(e =>
        {
            e.Property(p => p.CommercialName).HasMaxLength(200).IsRequired();
            e.Property(p => p.PrincipalActive).HasMaxLength(200).IsRequired();
            e.Property(p => p.Batch).HasMaxLength(50).IsRequired();
            e.Property(p => p.Price).HasColumnType("decimal(10,2)");
            e.HasIndex(p => new { p.Batch, p.ExpirationDate });

            e.HasOne(p => p.Laboratory)
                .WithMany(l => l.Medicines)
                .HasForeignKey(p => p.LaboratoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product <-> InventoryItem is 1:1, both entities declares an FK
            // to the other one (Product.InventoryId e InventoryItem.ProductId)
            // InventoryItem.ProductId is used as the real FK(InventoryItem = dependent); 
            //Product.InventoryId remains as simple column, without a FK constraint in DB
            e.HasOne(p => p.InventoryItem)
                .WithOne(i => i.Product)
                .HasForeignKey<InventoryItem>(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<InventoryItem>(e =>
        {
            e.Property(i => i.CurrentStock).IsRequired();
        });

        b.Entity<Customer>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(c => c.Email).IsUnique();

            e.HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Dispatcher>(e =>
        {
            e.Property(d => d.FirstName).HasMaxLength(100).IsRequired();
            e.Property(d => d.LastName).HasMaxLength(100).IsRequired();
            e.Property(d => d.Status).HasConversion<int>().IsRequired();
        });

        b.Entity<Order>(e =>
        {
            e.Property(o => o.TotalPrice).HasColumnType("decimal(10,2)");
            e.Property(o => o.Priority).HasConversion<int>().IsRequired();
            e.Property(o => o.Status).HasConversion<int>().IsRequired();
            e.Property(o => o.CreatedUtc).IsRequired();

            // Same navigation idea with 1 Order -> N Lines
            //Through OrderId as FK in OrderLine
            e.HasMany(o => o.Lines)
                .WithOne()
                .HasForeignKey(l => l.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(o => o.Status);
            e.HasIndex(o => o.Priority);
        });

        b.Entity<OrderLine>(e =>
        {
            e.Property(l => l.Units).IsRequired();

            // Restrict: delete a Product shouldn't eliminate in cascade
            // historic order lines that references it.
            e.HasOne<Product>()
                .WithMany()
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<FulfillmentEvent>(e =>
        {
            e.Property(f => f.Type).HasMaxLength(50).IsRequired();
            e.Property(f => f.FulfilledAtUtc).IsRequired();

            e.HasOne<Order>()
                .WithMany()
                .HasForeignKey(f => f.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Dispatcher>()
                .WithMany()
                .HasForeignKey(f => f.DispatcherId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(f => f.OrderId);
            e.HasIndex(f => f.DispatcherId);
        });

        // Setting our RowVersion property as an EF Core Row Version
        b.Entity<InventoryItem>().Property(i => i.RowVersion).IsRowVersion();
        b.Entity<Dispatcher>().Property(i => i.RowVersion).IsRowVersion();


        // Data Seeding

        b.Entity<Laboratory>().HasData(
            new Laboratory { Id = 1, Name = "Laboratorios Pisa", Email = "contacto@labpisa.com" },
            new Laboratory { Id = 2, Name = "Grupo Sanofi", Email = "contacto@sanofi.com" }
        );

        b.Entity<Product>().HasData(
            new Product { Id = 1, CommercialName = "Paracetamol Max", PrincipalActive = "Paracetamol", Dose = 500, ExpirationDate = new DateTime(2027, 6, 30), Batch = "PZ-1001", Price = 45.50m, LaboratoryId = 1, InventoryId = 1 },
            new Product { Id = 2, CommercialName = "Amoxil", PrincipalActive = "Amoxicilina", Dose = 250, ExpirationDate = new DateTime(2027, 3, 15), Batch = "PZ-1002", Price = 89.90m, LaboratoryId = 1, InventoryId = 2 },
            new Product { Id = 3, CommercialName = "Lipitor", PrincipalActive = "Atorvastatina", Dose = 20, ExpirationDate = new DateTime(2028, 1, 10), Batch = "SF-2001", Price = 210.00m, LaboratoryId = 2, InventoryId = 3 }
        );

        // RowVersion requires an explicit value when using HasData. Seeded value is a placeholder; it will be automatically overwritten on the first actual UPDATE of the row.
        b.Entity<InventoryItem>().HasData(
            new InventoryItem { Id = 1, CurrentStock = 120, ProductId = 1, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 } },
            new InventoryItem { Id = 2, CurrentStock = 8, ProductId = 2, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 2 } },
            new InventoryItem { Id = 3, CurrentStock = 45, ProductId = 3, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 3 } }
        );

        b.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "Farmacia del Centro", Email = "compras@farmaciacentro.com" },
            new Customer { Id = 2, Name = "Hospital San Rafael", Email = "compras@hospitalsanrafael.com" }
        );

        b.Entity<Dispatcher>().HasData(
            new Dispatcher { Id = 1, FirstName = "Laura", LastName = "Gómez", Status = DispatcherStatus.Free, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 } },
            new Dispatcher { Id = 2, FirstName = "Carlos", LastName = "Reyes", Status = DispatcherStatus.Busy, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 2 } }
        );

        b.Entity<Order>().HasData(
            new Order { Id = 1, CustomerId = 1, TotalPrice = 180.90m, Priority = OrderPriority.Normal, Status = OrderStatus.Fulfilled, CreatedUtc = new DateTime(2026, 6, 20, 10, 0, 0, DateTimeKind.Utc), CompletedUtc = new DateTime(2026, 6, 21, 15, 30, 0, DateTimeKind.Utc) },
            new Order { Id = 2, CustomerId = 2, TotalPrice = 210.00m, Priority = OrderPriority.Expedited, Status = OrderStatus.Pending, CreatedUtc = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc) }
        );

        b.Entity<OrderLine>().HasData(
            new OrderLine { Id = 1, OrderId = 1, ProductId = 1, Units = 2 },
            new OrderLine { Id = 2, OrderId = 1, ProductId = 2, Units = 1 },
            new OrderLine { Id = 3, OrderId = 2, ProductId = 3, Units = 1 }
        );

        b.Entity<FulfillmentEvent>().HasData(
            new FulfillmentEvent { Id = 1, OrderId = 1, DispatcherId = 1, Type = "Fulfilled", FulfilledAtUtc = new DateTime(2026, 6, 21, 15, 30, 0, DateTimeKind.Utc) }
        );
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateNewProductsExpirationDate();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateNewProductsExpirationDate();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Avoid inserting Products whose ExpirationDate has already passed at the time of the operation.
    // Only applies to new entries (Added): an existing product that expires over time can continue to be updated (e.g., price adjustment, or marked for inventory removal) without this rule blocking it.
    /// </summary>
    private void ValidateNewProductsExpirationDate()
    {
        var expiredNewProducts = ChangeTracker.Entries<Product>()
            .Where(entry => entry.State == EntityState.Added
                && entry.Entity.ExpirationDate.Date < DateTime.UtcNow.Date)
            .Select(entry => entry.Entity)
            .ToList();

        if (expiredNewProducts.Count > 0)
        {
            var batches = string.Join(", ", expiredNewProducts.Select(p => p.Batch));
            throw new DomainValidationException(
                $"Products with expired dates cannot be added. Affected batches: {batches}");
        }
    }
}