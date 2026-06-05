using MongoDB.Bson.Serialization.Conventions;

namespace Order.Infrastructure.Persistence;

public static class MongoDbConfiguration
{
    private static bool _configured;

    public static void Configure()
    {
        if (_configured)
            return;

        var conventions = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("DDD Conventions", conventions, _ => true);

        _configured = true;
    }
}

public class MongoDbSettings
{
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
}
