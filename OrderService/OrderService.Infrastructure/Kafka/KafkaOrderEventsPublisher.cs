using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using OrderService.Application.Orders.Events;
using OrderService.Application.Orders.Interfaces;

namespace OrderService.Infrastructure.Kafka;

public sealed class KafkaOrderEventsPublisher : IOrderEventsPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaOrderEventsPublisher(IConfiguration configuration)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topics:OrderPaymentRequested"] ?? "order-payment-requested";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishOrderPaymentRequestedAsync(OrderPaymentRequestedEvent evt, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(evt);
        await _producer.ProduceAsync(
            _topic,
            new Message<string, string> { Key = evt.OrderId.ToString(), Value = payload },
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}

