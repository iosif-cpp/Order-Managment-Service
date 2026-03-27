using AuthService.Application.Auth.Commands.Login;
using AuthService.Application.Auth.Interfaces;
using AuthService.Application.Auth;
using AuthService.Application.Common.Exceptions;
using AuthService.Domain.Entities;
using FluentAssertions;
using Moq;

namespace AuthService.Tests;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsInvalidCredentials()
    {
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        users.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var customerClient = new Mock<ICustomerServiceClient>(MockBehavior.Strict);

        var handler = new LoginCommandHandler(users.Object, hasher.Object, tokenService.Object, customerClient.Object);

        var act = async () => await handler.Handle(new LoginCommand("x@example.com", "pass"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ThrowsInvalidCredentials()
    {
        var user = new User("x@example.com", "x", "hash");

        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
        hasher.Setup(x => x.VerifyPassword("bad", user.PasswordHash))
            .Returns(false);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var customerClient = new Mock<ICustomerServiceClient>(MockBehavior.Strict);

        var handler = new LoginCommandHandler(users.Object, hasher.Object, tokenService.Object, customerClient.Object);

        var act = async () => await handler.Handle(new LoginCommand(user.Email, "bad"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_WhenValid_GeneratesTokensWithRolesFromCustomerService()
    {
        var user = new User("x@example.com", "x", "hash");
        var roles = new[] { "Customer", "Admin" };

        var expected = new AuthResponse
        {
            AccessToken = "access",
            RefreshToken = "refresh",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
        hasher.Setup(x => x.VerifyPassword("pass", user.PasswordHash))
            .Returns(true);

        var customerClient = new Mock<ICustomerServiceClient>(MockBehavior.Strict);
        customerClient
            .Setup(x => x.GetRolesByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService
            .Setup(x => x.GenerateTokensAsync(user, It.Is<IReadOnlyCollection<string>>(r => r.SequenceEqual(roles)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new LoginCommandHandler(users.Object, hasher.Object, tokenService.Object, customerClient.Object);

        var result = await handler.Handle(new LoginCommand(user.Email, "pass"), CancellationToken.None);

        result.Should().Be(expected);
        customerClient.Verify(x => x.GetRolesByEmailAsync(user.Email, It.IsAny<CancellationToken>()), Times.Once);
        tokenService.Verify(x => x.GenerateTokensAsync(user, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValid_CallsPasswordHasherWithStoredHash()
    {
        var user = new User("x@example.com", "x", "hash");
        var roles = Array.Empty<string>();

        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
        hasher.Setup(x => x.VerifyPassword("pass", user.PasswordHash))
            .Returns(true);

        var customerClient = new Mock<ICustomerServiceClient>(MockBehavior.Strict);
        customerClient
            .Setup(x => x.GetRolesByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService
            .Setup(x => x.GenerateTokensAsync(user, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponse { AccessToken = "a", RefreshToken = "r", ExpiresAt = DateTime.UtcNow });

        var handler = new LoginCommandHandler(users.Object, hasher.Object, tokenService.Object, customerClient.Object);

        _ = await handler.Handle(new LoginCommand(user.Email, "pass"), CancellationToken.None);

        hasher.Verify(x => x.VerifyPassword("pass", "hash"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValid_RequestsRolesByEmail()
    {
        var user = new User("x@example.com", "x", "hash");

        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
        hasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var customerClient = new Mock<ICustomerServiceClient>(MockBehavior.Strict);
        customerClient
            .Setup(x => x.GetRolesByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "Customer" });

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService
            .Setup(x => x.GenerateTokensAsync(It.IsAny<User>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponse { AccessToken = "a", RefreshToken = "r", ExpiresAt = DateTime.UtcNow });

        var handler = new LoginCommandHandler(users.Object, hasher.Object, tokenService.Object, customerClient.Object);

        _ = await handler.Handle(new LoginCommand(user.Email, "pass"), CancellationToken.None);

        customerClient.Verify(x => x.GetRolesByEmailAsync(user.Email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCustomerServiceReturnsNoRoles_GeneratesTokensWithEmptyRoles()
    {
        var user = new User("x@example.com", "x", "hash");

        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        users.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
        hasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var customerClient = new Mock<ICustomerServiceClient>(MockBehavior.Strict);
        customerClient
            .Setup(x => x.GetRolesByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService
            .Setup(x => x.GenerateTokensAsync(user, It.Is<IReadOnlyCollection<string>>(r => r.Count == 0), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponse { AccessToken = "a", RefreshToken = "r", ExpiresAt = DateTime.UtcNow });

        var handler = new LoginCommandHandler(users.Object, hasher.Object, tokenService.Object, customerClient.Object);

        _ = await handler.Handle(new LoginCommand(user.Email, "pass"), CancellationToken.None);

        tokenService.VerifyAll();
    }
}

