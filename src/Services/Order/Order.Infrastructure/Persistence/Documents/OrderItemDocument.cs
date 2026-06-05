namespace Order.Infrastructure.Persistence.Documents;

public class OrderItemDocument
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal UnitPriceAmount { get; set; }
    public string Currency { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal DiscountAmount { get; set; }
}
