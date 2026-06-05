using BuildingBlocks.Domain;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.ValueObjects;

namespace Order.Domain.Factories;

/// <summary>
/// Domain Factory - enforces invariants using domain language only.
/// No external types allowed here.
/// </summary>
public class OrderFactory(CreateOrderData data) : Factory<Aggregates.OrderAggregate.Order, OrderId>
{
    public Aggregates.OrderAggregate.Order Create()
    {
        return CreateWithValidation(() =>
        {
            var order = Aggregates.OrderAggregate.Order.Create(
                data.CustomerId,
                data.ShippingAddress,
                data.Currency
            );

            foreach (var item in data.Items)
            {
                order.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
            }

            if (!string.IsNullOrWhiteSpace(data.Notes))
            {
                order.SetNotes(data.Notes);
            }

            return order;
        });
    }

    protected override void Validate()
    {
        if (data.CustomerId == null)
        {
            throw new DomainException("Customer ID is required");
        }

        if (data.ShippingAddress == null)
        {
            throw new DomainException("Shipping address is required");
        }

        if (string.IsNullOrWhiteSpace(data.Currency))
        {
            throw new DomainException("Currency is required");
        }

        if (data.Items == null || !data.Items.Any())
        {
            throw new DomainException("Order must have at least one item");
        }

        if (data.Items.Count > 100)
        {
            throw new DomainException("Order cannot have more than 100 items");
        }

        foreach (var item in data.Items)
        {
            if (item.ProductId == null)
            {
                throw new DomainException("Product ID is required for all items");
            }

            if (string.IsNullOrWhiteSpace(item.ProductName))
            {
                throw new DomainException("Product name is required for all items");
            }

            if (item.UnitPrice == null || item.UnitPrice.Amount <= 0)
            {
                throw new DomainException("Unit price must be positive for all items");
            }

            if (item.Quantity <= 0)
            {
                throw new DomainException("Quantity must be positive for all items");
            }
        }
    }
}
