using BuildingBlocks.Domain.BusinessRules;
using MediatR;
using Order.Application.AntiCorruption;
using Order.Domain.BusinessRules;
using Order.Domain.Factories;
using Order.Domain.Repositories;

namespace Order.Application.Commands;

public record ImportExternalOrderCommand(
    string ExternalOrderId,
    Guid CustomerId,
    string CustomerName,
    string ShippingStreet,
    string ShippingCity,
    string ShippingState,
    string ShippingCountry,
    string ShippingZipCode,
    string Currency,
    List<ImportExternalOrderItemCommand> Items,
    string? Notes = null) : IRequest<Guid>;

public record ImportExternalOrderItemCommand(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

/// <summary>
/// Application Service - orchestrates the import flow:
/// 1. Validate input using BusinessRuleChecker (application-level validation)
/// 2. Translate external data (ACL)
/// 3. Create order via domain factory (domain invariants)
/// 4. Persist aggregate
///
/// NO business logic here - only orchestration and input validation.
/// </summary>
public class ImportExternalOrderCommandHandler(
    IOrderRepository orderRepository,
    ExternalOrderTranslator translator)
    : IRequestHandler<ImportExternalOrderCommand, Guid>
{
    public async Task<Guid> Handle(ImportExternalOrderCommand request, CancellationToken cancellationToken)
    {
        BusinessRuleChecker.ValidateAll(
            new CustomerIdMustBeProvidedRule(request.CustomerId),
            new ShippingAddressMustBeCompleteRule(
                request.ShippingStreet,
                request.ShippingCity,
                request.ShippingCountry,
                request.ShippingZipCode),
            new ImportedOrderMustHaveItemsRule(request.Items.Count),
            new ImportedItemsMustHaveValidPricesRule(request.Items.Select(i => i.UnitPrice)),
            new ImportedItemsMustHaveValidQuantitiesRule(request.Items.Select(i => i.Quantity))
        );

        var externalDto = new ExternalOrderDto(
            ExternalOrderId: request.ExternalOrderId,
            CustomerId: request.CustomerId,
            CustomerName: request.CustomerName,
            ShippingStreet: request.ShippingStreet,
            ShippingCity: request.ShippingCity,
            ShippingState: request.ShippingState,
            ShippingCountry: request.ShippingCountry,
            ShippingZipCode: request.ShippingZipCode,
            Currency: request.Currency,
            Items: request.Items.Select(i => new ExternalOrderItemDto(
                ProductId: i.ProductId,
                ProductName: i.ProductName,
                UnitPrice: i.UnitPrice,
                Quantity: i.Quantity
            )).ToList(),
            Notes: request.Notes
        );

        var createOrderData = translator.Translate(externalDto);

        var factory = new OrderFactory(createOrderData);
        var order = factory.Create();

        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return order.Id.Value;
    }
}
