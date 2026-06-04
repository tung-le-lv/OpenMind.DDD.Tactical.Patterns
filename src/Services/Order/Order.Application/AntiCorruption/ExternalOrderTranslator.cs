using Order.Domain.Factories;
using Order.Domain.ValueObjects;

namespace Order.Application.AntiCorruption;

/// <summary>
/// Anti-Corruption Layer (ACL) - Translates external order data to domain language.
/// 
/// This protects the domain from external system changes.
/// All translation, field mapping, and data conversion happens here.
/// NO business rules - only pure translation.
/// </summary>
public interface IExternalOrderTranslator
{
    CreateOrderData Translate(ExternalOrderDto externalDto);
}

public class ExternalOrderTranslator : IExternalOrderTranslator
{
    public CreateOrderData Translate(ExternalOrderDto external)
    {
        if (external == null)
            throw new ArgumentNullException(nameof(external));

        var customerId = CustomerId.From(external.CustomerId);
        var shippingAddress = new Address(
            external.ShippingStreet,
            external.ShippingCity,
            external.ShippingState,
            external.ShippingCountry,
            external.ShippingZipCode
        );

        var items = external.Items.Select(item => new OrderItemData(
            ProductId: ProductId.From(item.ProductId),
            ProductName: item.ProductName,
            UnitPrice: new Money(item.UnitPrice, external.Currency),
            Quantity: item.Quantity
        )).ToList();

        return new CreateOrderData(
            CustomerId: customerId,
            ShippingAddress: shippingAddress,
            Currency: external.Currency,
            Items: items,
            Notes: external.Notes
        );
    }
}

/// <summary>
/// External order DTO - represents data from external systems.
/// Lives in Application layer as part of ACL.
/// </summary>
public record ExternalOrderDto(
    string ExternalOrderId,
    Guid CustomerId,
    string CustomerName,
    string ShippingStreet,
    string ShippingCity,
    string ShippingState,
    string ShippingCountry,
    string ShippingZipCode,
    string Currency,
    List<ExternalOrderItemDto> Items,
    string? Notes = null
);

public record ExternalOrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);
