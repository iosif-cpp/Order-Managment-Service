namespace PaymentService.API.Responses;

public sealed record BalanceResponse(Guid CustomerId, decimal Amount);

