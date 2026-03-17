using FluentValidation;

namespace OrderService.Application.Orders.Commands.MarkOrderAsPaid;

public sealed class MarkOrderAsPaidCommandValidator : AbstractValidator<MarkOrderAsPaidCommand>
{
    public MarkOrderAsPaidCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}

