using FluentValidation;

namespace OrderService.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}

