using FluentValidation;

namespace OrderService.Application.Orders.Commands.ShipOrder;

public sealed class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}

