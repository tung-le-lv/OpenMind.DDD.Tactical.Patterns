using MongoDB.Bson.Serialization.Conventions;

namespace Payment.Infrastructure.Persistence;

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
