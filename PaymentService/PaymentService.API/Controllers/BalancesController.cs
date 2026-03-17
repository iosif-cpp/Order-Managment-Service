using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.API.Requests;
using PaymentService.API.Responses;
using PaymentService.Application.Balances.Commands.CreditBalance;
using PaymentService.Application.Balances.Commands.DebitBalance;
using PaymentService.Application.Balances.Queries.GetBalance;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class BalancesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpClientFactory _httpClientFactory;

    public BalancesController(IMediator mediator, IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("{customerId:guid}")]
    public async Task<ActionResult<BalanceResponse>> GetByCustomerId(Guid customerId, CancellationToken cancellationToken)
    {
        var authorized = await EnsureBalanceAccessAsync(customerId, cancellationToken);
        if (authorized is not null)
            return authorized;

        var result = await _mediator.Send(new GetBalanceQuery(customerId), cancellationToken);
        return Ok(new BalanceResponse(result.CustomerId, result.Amount));
    }

    [HttpPost("{customerId:guid}/credit")]
    public async Task<ActionResult<BalanceResponse>> Credit(
        Guid customerId,
        [FromBody] CreditBalanceRequest request,
        CancellationToken cancellationToken)
    {
        var authorized = await EnsureBalanceAccessAsync(customerId, cancellationToken);
        if (authorized is not null)
            return authorized;

        var result = await _mediator.Send(new CreditBalanceCommand(customerId, request.Amount), cancellationToken);
        return Ok(new BalanceResponse(result.CustomerId, result.Amount));
    }

    [HttpPost("{customerId:guid}/debit")]
    public async Task<ActionResult<BalanceResponse>> Debit(
        Guid customerId,
        [FromBody] DebitBalanceRequest request,
        CancellationToken cancellationToken)
    {
        var authorized = await EnsureBalanceAccessAsync(customerId, cancellationToken);
        if (authorized is not null)
            return authorized;

        var result = await _mediator.Send(new DebitBalanceCommand(customerId, request.Amount), cancellationToken);
        return Ok(new BalanceResponse(result.CustomerId, result.Amount));
    }

    private async Task<ActionResult?> EnsureBalanceAccessAsync(Guid customerId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Admin"))
            return null;

        var email =
            User.FindFirstValue(ClaimTypes.Email) ??
            User.FindFirstValue("email");

        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized();

        var http = _httpClientFactory.CreateClient("CustomerService");
        var encodedEmail = WebUtility.UrlEncode(email);

        HttpResponseMessage response;
        try
        {
            response = await http.GetAsync($"/api/customers/by-email/{encodedEmail}", cancellationToken);
        }
        catch
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            return StatusCode(StatusCodes.Status503ServiceUnavailable);

        if (!response.IsSuccessStatusCode)
            return Forbid();

        CustomerResponse? customer;
        try
        {
            customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken);
        }
        catch
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        if (customer is null || customer.Id == Guid.Empty)
            return Forbid();

        return customer.Id == customerId ? null : Forbid();
    }

    private sealed record CustomerResponse(
        Guid Id,
        string Email,
        string UserName,
        IReadOnlyCollection<string> Roles);
}
