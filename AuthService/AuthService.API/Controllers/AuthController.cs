using AuthService.Application.Auth;
using AuthService.Application.Auth.Commands.Login;
using AuthService.Application.Auth.Commands.RefreshToken;
using AuthService.Application.Auth.Commands.Logout;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request.RefreshToken);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
