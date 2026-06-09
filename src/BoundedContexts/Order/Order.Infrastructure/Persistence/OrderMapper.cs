using BuildingBlocks.Domain;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Persistence.Documents;

namespace Order.Infrastructure.Persistence;

public static class OrderMapper
{
    public static OrderDocument ToDocument(Domain.Aggregates.OrderAggregate.Order order) => new()
    {
        Id          = order.Id.Value,
        CustomerId  = order.CustomerId.Value,
        Street      = order.ShippingAddress.Street,
        City        = order.ShippingAddress.City,
        State       = order.ShippingAddress.State,
        Country     = order.ShippingAddress.Country,
        ZipCode     = order.ShippingAddress.ZipCode,
        Status      = order.Status.Name,
        CreatedAt   = order.CreatedAt,
        ModifiedAt  = order.ModifiedAt,
        SubmittedAt = order.SubmittedAt,
        PaidAt      = order.PaidAt,
        Notes       = order.Notes,
        Currency    = order.Currency,
        Version     = order.Version,
        OrderItems  = order.OrderItems.Select(ToItemDocument).ToList()
    };

    public static Domain.Aggregates.OrderAggregate.Order ToDomain(OrderDocument doc) =>
        Domain.Aggregates.OrderAggregate.Order.Reconstitute(
            id:              OrderId.From(doc.Id),
            customerId:      CustomerId.From(doc.CustomerId),
            shippingAddress: new Address(doc.Street, doc.City, doc.State, doc.Country, doc.ZipCode),
            status:          Enumeration.FromDisplayName<OrderStatus>(doc.Status),
            createdAt:       doc.CreatedAt,
            modifiedAt:      doc.ModifiedAt,
            submittedAt:     doc.SubmittedAt,
            paidAt:          doc.PaidAt,
            notes:           doc.Notes,
            currency:        doc.Currency,
            version:         doc.Version,
            orderItems:      doc.OrderItems.Select(ToItemDomain)
        );

    private static OrderItemDocument ToItemDocument(OrderItem item) => new()
    {
        Id              = item.Id.Value,
        ProductId       = item.ProductId.Value,
        ProductName     = item.ProductName,
        UnitPriceAmount = item.UnitPrice.Amount,
        Currency        = item.UnitPrice.Currency,
        Quantity        = item.Quantity,
        DiscountAmount  = item.Discount.Amount
    };

    private static OrderItem ToItemDomain(OrderItemDocument doc) =>
        OrderItem.Reconstitute(
            id:          OrderItemId.From(doc.Id),
            productId:   ProductId.From(doc.ProductId),
            productName: doc.ProductName,
            unitPrice:   new Money(doc.UnitPriceAmount, doc.Currency),
            quantity:    doc.Quantity,
            discount:    new Money(doc.DiscountAmount, doc.Currency)
        );
}
