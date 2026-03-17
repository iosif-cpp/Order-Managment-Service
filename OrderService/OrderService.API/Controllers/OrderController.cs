using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Orders;
using OrderService.Application.Orders.Commands.CreateOrder;
using OrderService.Application.Orders.Commands.CancelOrder;
using OrderService.Application.Orders.Commands.MarkOrderAsPaid;
using OrderService.Application.Orders.Commands.ShipOrder;
using OrderService.Application.Orders.Queries.GetOrderById;
using OrderService.Application.Orders.Queries.GetOrdersForCustomer;
using System.Net;
using System.Net.Http.Json;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrderController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;

    public OrderController(IMediator mediator, IMapper mapper, IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = GetCurrentUserId();

        var command = _mapper.Map<CreateOrderCommand>(request) with
        {
            CustomerId = customerId
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var customerId = GetCurrentUserId();
        var query = new GetOrdersForCustomerQuery(customerId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var customerId = GetCurrentUserId();
        var command = new CancelOrderCommand(id, customerId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsPaid(Guid id, CancellationToken cancellationToken)
    {
        var customerId = GetCurrentUserId();
        var command = new MarkOrderAsPaidCommand(id, customerId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Ship(Guid id, CancellationToken cancellationToken)
    {
        var customerId = GetCurrentUserId();
        var command = new ShipOrderCommand(id, customerId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Email is missing in token.");

        var http = _httpClientFactory.CreateClient("CustomerService");
        var encoded = WebUtility.UrlEncode(email);

        var response = http.GetAsync($"/api/customers/by-email/{encoded}").GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Customer service is unavailable.");

        var customer = response.Content.ReadFromJsonAsync<CustomerResponse>().GetAwaiter().GetResult();
        if (customer is null || customer.Id == Guid.Empty)
            throw new InvalidOperationException("Customer is not found.");

        return customer.Id;
    }

    private sealed record CustomerResponse(
        Guid Id,
        string Email,
        string UserName,
        IReadOnlyCollection<string> Roles);
}
