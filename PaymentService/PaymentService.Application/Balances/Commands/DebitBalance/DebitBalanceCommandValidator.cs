using FluentValidation;

namespace PaymentService.Application.Balances.Commands.DebitBalance;

public sealed class DebitBalanceCommandValidator : AbstractValidator<DebitBalanceCommand>
{
    public DebitBalanceCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0m);
    }
}

