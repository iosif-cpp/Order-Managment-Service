using CustomerService.Application.Common.Exceptions;
using CustomerService.Application.Users.Commands.RegisterUser;
using CustomerService.Application.Users.Events;
using CustomerService.Application.Users.Interfaces;
using CustomerService.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CustomerService.Tests;

public sealed class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEmailExists_ThrowsEmailAlreadyExistsException()
    {
        var repo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("a@b.com", "u"));

        var roles = new Mock<IRoleRepository>(MockBehavior.Strict);
        var publisher = new Mock<ICustomerEventsPublisher>(MockBehavior.Strict);

        var handler = new RegisterUserCommandHandler(repo.Object, roles.Object, publisher.Object);

        var act = async () => await handler.Handle(new RegisterUserCommand("a@b.com", "u", "p"), CancellationToken.None);

        await act.Should().ThrowAsync<EmailAlreadyExistsException>();
    }

    [Fact]
    public async Task Handle_WhenDefaultRoleMissing_CreatesRoleAndPublishesEvent()
    {
        var repo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        repo.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var roles = new Mock<IRoleRepository>(MockBehavior.Strict);
        roles.Setup(x => x.GetByNameAsync("Customer", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);
        roles.Setup(x => x.AddAsync(It.Is<Role>(r => r.Name == "Customer"), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        CustomerRegisteredEvent? published = null;
        var publisher = new Mock<ICustomerEventsPublisher>(MockBehavior.Strict);
        publisher.Setup(x => x.PublishCustomerRegisteredAsync(It.IsAny<CustomerRegisteredEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CustomerRegisteredEvent, CancellationToken>((e, _) => published = e)
            .Returns(Task.CompletedTask);

        var handler = new RegisterUserCommandHandler(repo.Object, roles.Object, publisher.Object);

        var userId = await handler.Handle(new RegisterUserCommand("a@b.com", "u", "p"), CancellationToken.None);

        userId.Should().NotBe(Guid.Empty);
        published.Should().NotBeNull();
        published!.Email.Should().Be("a@b.com");
        published.UserName.Should().Be("u");
        published.Password.Should().Be("p");
        published.Roles.Should().ContainSingle().Which.Should().Be("Customer");

        roles.VerifyAll();
        repo.VerifyAll();
        publisher.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenDefaultRoleExists_DoesNotCreateRole()
    {
        var repo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        repo.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var role = new Role("Customer");
        var roles = new Mock<IRoleRepository>(MockBehavior.Strict);
        roles.Setup(x => x.GetByNameAsync("Customer", It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var publisher = new Mock<ICustomerEventsPublisher>(MockBehavior.Strict);
        publisher.Setup(x => x.PublishCustomerRegisteredAsync(It.IsAny<CustomerRegisteredEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RegisterUserCommandHandler(repo.Object, roles.Object, publisher.Object);

        _ = await handler.Handle(new RegisterUserCommand("a@b.com", "u", "p"), CancellationToken.None);

        roles.Verify(x => x.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValid_AddsUserAndSavesChanges()
    {
        var repo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        repo.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var roles = new Mock<IRoleRepository>(MockBehavior.Strict);
        roles.Setup(x => x.GetByNameAsync("Customer", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role("Customer"));

        var publisher = new Mock<ICustomerEventsPublisher>(MockBehavior.Strict);
        publisher.Setup(x => x.PublishCustomerRegisteredAsync(It.IsAny<CustomerRegisteredEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RegisterUserCommandHandler(repo.Object, roles.Object, publisher.Object);

        _ = await handler.Handle(new RegisterUserCommand("a@b.com", "u", "p"), CancellationToken.None);

        repo.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValid_PublishesEventWithUserIdFromCreatedUser()
    {
        User? created = null;

        var repo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        repo.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => created = u)
            .Returns(Task.CompletedTask);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var roles = new Mock<IRoleRepository>(MockBehavior.Strict);
        roles.Setup(x => x.GetByNameAsync("Customer", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role("Customer"));

        CustomerRegisteredEvent? published = null;
        var publisher = new Mock<ICustomerEventsPublisher>(MockBehavior.Strict);
        publisher.Setup(x => x.PublishCustomerRegisteredAsync(It.IsAny<CustomerRegisteredEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CustomerRegisteredEvent, CancellationToken>((e, _) => published = e)
            .Returns(Task.CompletedTask);

        var handler = new RegisterUserCommandHandler(repo.Object, roles.Object, publisher.Object);

        var id = await handler.Handle(new RegisterUserCommand("a@b.com", "u", "p"), CancellationToken.None);

        created.Should().NotBeNull();
        created!.Id.Should().Be(id);

        published.Should().NotBeNull();
        published!.UserId.Should().Be(id);
    }
}

