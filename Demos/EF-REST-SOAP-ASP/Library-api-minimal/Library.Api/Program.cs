using Microsoft.EntityFrameworkCore;
using Library.Data;

//My API program.cs
//No main. We can think of it as 2 sections
//Registering things with the builder
//And the configuring things on the app
//And at the very bottom that app object that represents our entire API alls ots run method

//Builder area
var builder = WebApplication.CreateBuilder(args);

// The first thing that we need is to give our builder a connection string to a database
var conn_string = "Server=localhost,1433;Database=LibraryMinimalDb;User Id=sa;Password=LibraryPass1!;TrustServerCertificate=true";

//Tell the builder to use our LibraryDbContext with the connection string above
//By registering our DbContext class
//we hand off the managing of creating and destroying theses DbContext object to ASP.NET's
//dependency injection container. Like spring beans if you're familiar.
builder.Services.AddDbContext<LibraryDbContext>(options => options.UseSqlServer(conn_string));

//Swagger stuff added o builder
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// App area
var app = builder.Build();

//Swagger stuff aded to app
app.UseSwagger();
app.UseSwaggerUI();

//Endpoint area
app.MapGet("/", () => "Hello World!");

//Get all items from the inventory
app.MapGet("/inventory", async (LibraryDbContext db) =>
{
    return await db.Inventory.ToListAsync();
});

//Lets use LINQ - Language Integrated Query
// LINQ is a library that just lets us query collections
// The logic actually flows from SQL  DQL - You can use method OR sql query syntax
// You can even save the queries themselves as C# objects if you want to
app.MapGet("/inventory/by-value", (LibraryDbContext db) =>
{
    return db.Inventory.Include(i => i.Product)
        .GroupBy(i => i.CurrentStock >= 5 ? "well-stocked" : "low")
        .Select(g => new { tier = g.Key, cout = g.Count(), units = g.Sum(i => i.CurrentStock) })
        .ToList();
});

//My file always end with app.Run() - minimal API or Controller API
app.Run();
