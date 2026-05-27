using BuildingBlocks.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Queries;

namespace Order.API.Controllers;

/// <summary>
/// API Controller for Order operations.
/// Follows CQRS pattern - Commands modify state, Queries read state.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator, ILogger<OrdersController> logger) : ControllerBase
{
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery { OrderId = orderId };
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
    
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var query = new GetOrdersByCustomerQuery { CustomerId = customerId };
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken cancellationToken)
    {
        var query = new GetOrdersByStatusQuery { Status = status };
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var query = new GetPendingOrdersQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand
        {
            CustomerId = request.CustomerId,
            ShippingAddress = request.ShippingAddress,
            Currency = request.Currency,
            Notes = request.Notes
        };

        var orderId = await mediator.Send(command, cancellationToken);

        logger.LogInformation("Order {OrderId} created", orderId);

        return CreatedAtAction(nameof(GetById), new { orderId }, orderId);
    }
    
    [HttpPost("{orderId:guid}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem(
        Guid orderId,
        [FromBody] AddOrderItemDto request,
        CancellationToken cancellationToken)
    {
        var command = new AddOrderItemCommand
        {
            OrderId = orderId,
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            UnitPrice = request.UnitPrice,
            Quantity = request.Quantity
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound();

        return Ok();
    }
    
    [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid itemId, CancellationToken cancellationToken)
    {
        var command = new RemoveOrderItemCommand
        {
            OrderId = orderId,
            ItemId = itemId
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound();

        return Ok();
    }
    
    [HttpPut("{orderId:guid}/address")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(
        Guid orderId,
        [FromBody] AddressDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateShippingAddressCommand
        {
            OrderId = orderId,
            NewAddress = request
        };

        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound();

        return Ok();
    }
    
    [HttpPost("{orderId:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(Guid orderId, CancellationToken cancellationToken)
    {
        var command = new SubmitOrderCommand { OrderId = orderId };

        try
        {
            var result = await mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound();

            logger.LogInformation("Order {OrderId} submitted", orderId);

            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(
        Guid orderId,
        [FromBody] CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand
        {
            OrderId = orderId,
            Reason = request.Reason
        };

        try
        {
            var result = await mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound();

            logger.LogInformation("Order {OrderId} cancelled. Reason: {Reason}", orderId, request.Reason);

            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{targetOrderId:guid}/consolidate/{sourceOrderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Consolidate(
        Guid targetOrderId,
        Guid sourceOrderId,
        CancellationToken cancellationToken)
    {
        var command = new ConsolidateOrdersCommand
        {
            SourceOrderId = sourceOrderId,
            TargetOrderId = targetOrderId
        };

        try
        {
            var result = await mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound();

            logger.LogInformation("Order {SourceOrderId} consolidated into {TargetOrderId}", sourceOrderId, targetOrderId);

            return Ok();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportExternal(
        [FromBody] ImportExternalOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ImportExternalOrderCommand
        {
            ExternalOrderId = request.ExternalOrderId,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            ShippingStreet = request.ShippingStreet,
            ShippingCity = request.ShippingCity,
            ShippingState = request.ShippingState,
            ShippingCountry = request.ShippingCountry,
            ShippingZipCode = request.ShippingZipCode,
            Currency = request.Currency,
            Items = request.Items.Select(i => new ImportExternalOrderItemCommand
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList(),
            Notes = request.Notes
        };

        var orderId = await mediator.Send(command, cancellationToken);

        logger.LogInformation("External order {ExternalOrderId} imported as {OrderId}", request.ExternalOrderId, orderId);

        return CreatedAtAction(nameof(GetById), new { orderId }, orderId);
    }
}

public record CancelOrderRequest(string Reason);

public record ImportExternalOrderRequest(
    string ExternalOrderId,
    Guid CustomerId,
    string CustomerName,
    string ShippingStreet,
    string ShippingCity,
    string ShippingState,
    string ShippingCountry,
    string ShippingZipCode,
    string Currency,
    List<ImportExternalOrderItemRequest> Items,
    string? Notes = null
);

public record ImportExternalOrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);
