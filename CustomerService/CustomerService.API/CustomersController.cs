using CustomerService.API.Requests;
using CustomerService.Application.Users.Commands.AssignRole;
using CustomerService.Application.Users.Commands.RegisterUser;
using CustomerService.Application.Users.Queries.GetUserWithRoles;
using CustomerService.Application.Users.Queries.GetUserWithRolesByEmail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.API;

public sealed record CustomerResponse(
    Guid Id,
    string Email,
    string UserName,
    IReadOnlyCollection<string> Roles);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.Email, request.UserName, request.Password);
        var id = await _mediator.Send(command, cancellationToken);

        var query = new GetUserWithRolesQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        var response = new CustomerResponse(result.Id, result.Email, result.UserName, result.Roles);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserWithRolesQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        var response = new CustomerResponse(result.Id, result.Email, result.UserName, result.Roles);
        return Ok(response);
    }

    [HttpGet("by-email/{email}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(new GetUserWithRolesByEmailQuery(email), cancellationToken);
        var response = new CustomerResponse(user.Id, user.Email, user.UserName, user.Roles);
        return Ok(response);
    }
    
    [HttpPost("{id:guid}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GrantRole(
        Guid id,
        [FromBody] ChangeRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignRoleCommand(id, request.RoleName);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
