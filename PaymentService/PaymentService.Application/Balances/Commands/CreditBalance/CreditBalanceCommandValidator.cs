using FluentValidation;

namespace PaymentService.Application.Balances.Commands.CreditBalance;

public sealed class CreditBalanceCommandValidator : AbstractValidator<CreditBalanceCommand>
{
    public CreditBalanceCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0m);
    }
}

