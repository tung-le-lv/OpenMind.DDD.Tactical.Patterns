using MongoDB.Bson.Serialization.Attributes;

namespace Order.Search.ReadModels;

[BsonIgnoreExtraElements]
public class OrderReadModel
{
    [BsonId]
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public int Version { get; set; }
    public List<OrderItemReadModel> OrderItems { get; set; } = [];

    public decimal TotalAmount =>
        OrderItems.Sum(i => i.UnitPriceAmount * i.Quantity - i.DiscountAmount);
}

[BsonIgnoreExtraElements]
public class OrderItemReadModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal DiscountAmount { get; set; }

    public decimal LineTotal => UnitPriceAmount * Quantity - DiscountAmount;
}
