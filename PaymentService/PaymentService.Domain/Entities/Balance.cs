namespace PaymentService.Domain.Entities;

public sealed class Balance
{
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Balance()
    {
    }

    public Balance(Guid customerId, decimal initialAmount = 0m)
    {
        CustomerId = customerId;
        Amount = initialAmount;
        CreatedAt = DateTime.UtcNow;
    }

    public void Credit(decimal value)
    {
        Amount += value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Debit(decimal value)
    {
        Amount -= value;
        UpdatedAt = DateTime.UtcNow;
    }
}

