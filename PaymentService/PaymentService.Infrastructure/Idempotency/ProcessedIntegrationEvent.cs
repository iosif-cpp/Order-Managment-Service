namespace PaymentService.Infrastructure.Idempotency;

public sealed class ProcessedIntegrationEvent
{
    public string EventId { get; private set; } = null!;
    public DateTime ProcessedAt { get; private set; }

    private ProcessedIntegrationEvent()
    {
    }

    public ProcessedIntegrationEvent(string eventId)
    {
        EventId = eventId;
        ProcessedAt = DateTime.UtcNow;
    }
}

