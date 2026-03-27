using FluentAssertions;
using Moq;
using PaymentService.Application.Balances.Commands.CreditBalance;
using PaymentService.Application.Balances.Commands.DebitBalance;
using PaymentService.Application.Balances.Interfaces;
using PaymentService.Application.Balances.Queries.GetBalance;
using PaymentService.Application.Common.Exceptions;
using PaymentService.Domain.Entities;

namespace PaymentService.Tests;

public sealed class DebitBalanceCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBalanceNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new Mock<IBalanceRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByCustomerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Balance?)null);

        var handler = new DebitBalanceCommandHandler(repo.Object);

        var act = async () => await handler.Handle(new DebitBalanceCommand(Guid.NewGuid(), 10m), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenInsufficientFunds_ThrowsInsufficientFundsException()
    {
        var balance = new Balance(Guid.NewGuid(), 5m);

        var repo = new Mock<IBalanceRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByCustomerIdAsync(balance.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        var handler = new DebitBalanceCommandHandler(repo.Object);

        var act = async () => await handler.Handle(new DebitBalanceCommand(balance.CustomerId, 10m), CancellationToken.None);

        await act.Should().ThrowAsync<InsufficientFundsException>();
    }

    [Fact]
    public async Task Handle_WhenValid_DebitsAndSaves()
    {
        var balance = new Balance(Guid.NewGuid(), 100m);

        var repo = new Mock<IBalanceRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByCustomerIdAsync(balance.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new DebitBalanceCommandHandler(repo.Object);

        var result = await handler.Handle(new DebitBalanceCommand(balance.CustomerId, 30m), CancellationToken.None);

        result.CustomerId.Should().Be(balance.CustomerId);
        result.Amount.Should().Be(70m);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Credit_WhenBalanceNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new Mock<IBalanceRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByCustomerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Balance?)null);

        var handler = new CreditBalanceCommandHandler(repo.Object);

        var act = async () => await handler.Handle(new CreditBalanceCommand(Guid.NewGuid(), 10m), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Credit_WhenValid_CreditsAndSaves()
    {
        var balance = new Balance(Guid.NewGuid(), 5m);

        var repo = new Mock<IBalanceRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByCustomerIdAsync(balance.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreditBalanceCommandHandler(repo.Object);

        var result = await handler.Handle(new CreditBalanceCommand(balance.CustomerId, 10m), CancellationToken.None);

        result.Amount.Should().Be(15m);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBalance_WhenBalanceNotFound_ThrowsEntityNotFoundException()
    {
        var repo = new Mock<IBalanceRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByCustomerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Balance?)null);

        var handler = new GetBalanceQueryHandler(repo.Object);

        var act = async () => await handler.Handle(new GetBalanceQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetBalance_WhenFound_ReturnsBalance()
    {
        var balance = new Balance(Guid.NewGuid(), 123m);

        var repo = new Mock<IBalanceRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetByCustomerIdAsync(balance.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        var handler = new GetBalanceQueryHandler(repo.Object);

        var result = await handler.Handle(new GetBalanceQuery(balance.CustomerId), CancellationToken.None);

        result.CustomerId.Should().Be(balance.CustomerId);
        result.Amount.Should().Be(123m);
    }
}

