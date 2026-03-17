using System.Text.Json;
using Confluent.Kafka;
using CustomerService.Application.Users.Events;
using CustomerService.Application.Users.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CustomerService.Infrastructure.Kafka;

public sealed class KafkaCustomerEventsPublisher : ICustomerEventsPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaCustomerEventsPublisher(IConfiguration configuration)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topics:CustomerRegistered"] ?? "customer-registered";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishCustomerRegisteredAsync(CustomerRegisteredEvent evt, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(evt);
        var message = new Message<string, string>
        {
            Key = evt.UserId.ToString(),
            Value = json
        };

        await _producer.ProduceAsync(_topic, message, ct);
    }

    public void Dispose()
    {
        _producer.Flush();
        _producer.Dispose();
    }
}

