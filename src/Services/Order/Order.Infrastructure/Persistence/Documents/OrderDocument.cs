using MongoDB.Bson.Serialization.Attributes;

namespace Order.Infrastructure.Persistence.Documents;

public class OrderDocument
{
    [BsonId]
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public string Currency { get; set; } = default!;
    public int Version { get; set; }
    public List<OrderItemDocument> OrderItems { get; set; } = [];
}
