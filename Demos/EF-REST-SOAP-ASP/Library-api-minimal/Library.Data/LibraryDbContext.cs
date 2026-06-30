using Microsoft.EntityFrameworkCore;
using Library.Data.Entities;

namespace Library.Data;

//ALL of the code that does the actual SQL generation, creating a connection to my database,
//doing CRUD, updating the DB based on changes to my model - ALL OF THAT lives in a class
//called DbContext. I don't want to modify that class. It comes in from EF Core itself. What I do 
//is create a file with a class that INHERITS from it
public class LibraryDbContext : DbContext
{

    //This class need a constructor, and it need to take a certain argument
    //We ourselves will never call this constructor. ASP .NET's DI Container will do it for us
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    //We need to tell oir DbContext what C# classes we are tracking as Entities
    //Reminder - these Entitites becoume our tables. We register the entitites here.
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();

    //If I want to do things like deepear configurations options or data seeding
    //I can override a method we inherited from DbContext
    //called OnModelCreating() - this is called when EF Core creates a migration
    protected override void OnModelCreating(ModelBuilder b)
    {
        //I can set anything I want as far constraints, mapping column name and types
        //inside of here using something called Fluent API. EF Core lets you do config
        //in 3 ways. Convention < Data Annotations < FluentAPI on OnModelCreating

        //For example here is the same config we did by convention and annotation prior
        b.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Sku).IsUnique();

            e.Property(p => p.Price).HasColumnType("decimal(10,2)");

            e.HasOne(p => p.Inventory)
                .WithOne(i => i.Product)
                .HasForeignKey<InventoryItem>(i => i.ProductId);
        });

        //After you configure your entities
        //we can use OnModelCreating to seed data
        b.Entity<Product>().HasData(
            new Product { Id = 1, Sku = "BK-001", Name = "Clean Code", Price = 32.00M },
            new Product { Id = 2, Sku = "BK-002", Name = "The Pragmatic Programmer", Price = 38.00M },
            new Product { Id = 3, Sku = "BK-003", Name = "Refactoring", Price = 45.00M }
        );

        b.Entity<InventoryItem>().HasData(
            new InventoryItem { Id = 1, ProductId = 1, CurrentStock = 5 },
            new InventoryItem { Id = 2, ProductId = 2, CurrentStock = 5 },
            new InventoryItem { Id = 3, ProductId = 3, CurrentStock = 5 }
        );
    }
}