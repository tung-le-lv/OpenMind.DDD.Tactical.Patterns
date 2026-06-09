using MongoDB.Bson.Serialization.Conventions;

namespace Order.Search.Infrastructure;

public static class MongoDbConfiguration
{
    private static bool _configured;

    public static void Configure()
    {
        if (_configured)
        {
            return;
        }

        var conventions = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("OrderSearch Conventions", conventions, _ => true);

        _configured = true;
    }
}

public class MongoDbSettings
{
    public const string SectionName = "MongoDB";
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
