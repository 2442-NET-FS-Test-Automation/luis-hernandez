using Microsoft.EntityFrameworkCore;
using Pharmacy.Api.Queueing;
using Pharmacy.Api.Services;
using Pharmacy.Data.Configurations;

namespace Pharmacy.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers data access with two distinct lifecycles:
    /// 1) PharmacyDbContext (Scoped, via AddDbContext): one instance per
    /// request.This is what is injected into the vast majority of endpoints.
    /// 2) IDbContextFactory<PharmacyDbContext> (via AddDbContextFactory):
    /// factory that creates new instances of PharmacyDbContext on demand.
    /// Used when the same operation needs to execute several queries
    /// in parallel(Task.WhenAll, background jobs, etc.), since a single
    /// DbContext is not thread - safe and cannot be shared between concurrent tasks.
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la connection string 'DefaultConnection'.");

        // DbContext remains Scoped per HTTP request.
        // DbContextOptions are Singleton so they can be safely shared with
        // IDbContextFactory (registered as Singleton).
        services.AddDbContext<PharmacyDbContext>(
        options =>
        {
            options.UseSqlServer(connectionString);
        },
        contextLifetime: ServiceLifetime.Scoped,
        optionsLifetime: ServiceLifetime.Singleton);

        services.AddDbContextFactory<PharmacyDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        return services;
    }

    /// <summary>
    /// Here application services (business logic) are registered
    /// They are registered as Scoped, since they will depend on PharmacyDbContext
    /// (also Scoped).
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IFulfillmentService, FulfillmentService>();
        services.AddScoped<OrderFactory>();
        services.AddScoped<BurstPlanner>();
        services.AddScoped<ISeeder, Seeder>();
        services.AddSingleton<PriorityOrderQueue>();
        services.AddHostedService<DispatcherWorkerService>();

        return services;
    }
}