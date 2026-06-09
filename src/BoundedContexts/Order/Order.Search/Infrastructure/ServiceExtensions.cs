using Microsoft.Extensions.DependencyInjection;
using Order.Search.Infrastructure.Persistence;

namespace Order.Search.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddOrderSearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        MongoDbConfiguration.Configure();

        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<OrderSearchMongoDbContext>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceExtensions).Assembly));

        return services;
    }

    public static async Task InitializeOrderSearchAsync(this IServiceProvider services)
    {
        var context = services.GetRequiredService<OrderSearchMongoDbContext>();
        await context.EnsureIndexesAsync();
    }
}
