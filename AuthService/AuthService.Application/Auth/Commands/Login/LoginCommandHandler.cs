using AuthService.Application.Auth.Interfaces;
using AuthService.Application.Common.Exceptions;
using MediatR;

namespace AuthService.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ICustomerServiceClient _customerClient;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ICustomerServiceClient customerClient)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _customerClient = customerClient;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        var isValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isValid)
        {
            throw new InvalidCredentialsException();
        }

        var roles = await _customerClient.GetRolesByEmailAsync(user.Email, cancellationToken);

        var tokens = await _tokenService.GenerateTokensAsync(user, roles, cancellationToken);

        return tokens;
    }
}

