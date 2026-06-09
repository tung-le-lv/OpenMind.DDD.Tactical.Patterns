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
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(orderId), cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrdersByCustomerQuery(customerId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrdersByStatusQuery(status), cancellationToken);
        return Ok(result);
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPendingOrdersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto request, CancellationToken cancellationToken)
    {
        var orderId = await mediator.Send(
            new CreateOrderCommand(request.CustomerId, request.ShippingAddress, request.Currency, request.Notes),
            cancellationToken);

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
        var result = await mediator.Send(
            new AddOrderItemCommand(orderId, request.ProductId, request.ProductName, request.UnitPrice, request.Quantity),
            cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid itemId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveOrderItemCommand(orderId, itemId), cancellationToken);

        if (!result)
        {
            return NotFound();
        }

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
        var result = await mediator.Send(new UpdateShippingAddressCommand(orderId, request), cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPost("{orderId:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new SubmitOrderCommand(orderId), cancellationToken);

            if (!result)
            {
                return NotFound();
            }

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
        try
        {
            var result = await mediator.Send(new CancelOrderCommand(orderId, request.Reason), cancellationToken);

            if (!result)
            {
                return NotFound();
            }

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
        try
        {
            var result = await mediator.Send(new ConsolidateOrdersCommand(sourceOrderId, targetOrderId), cancellationToken);

            if (!result)
            {
                return NotFound();
            }

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
        var command = new ImportExternalOrderCommand(
            request.ExternalOrderId,
            request.CustomerId,
            request.CustomerName,
            request.ShippingStreet,
            request.ShippingCity,
            request.ShippingState,
            request.ShippingCountry,
            request.ShippingZipCode,
            request.Currency,
            request.Items.Select(i => new ImportExternalOrderItemCommand(
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity)).ToList(),
            request.Notes);

        var orderId = await mediator.Send(command, cancellationToken);

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
    string? Notes = null);

public record ImportExternalOrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
