using System.Text.Json;
using CatalogService.Application.Products.Events;
using CatalogService.Application.Products.Interfaces;
using CatalogService.Domain.Entities;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace CatalogService.Infrastructure.Kafka;

public sealed class KafkaProductEventsPublisher : IProductEventsPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaProductEventsPublisher(IConfiguration configuration)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topics:ProductUpserted"] ?? "product-upserted";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishProductUpsertedAsync(Product product, CancellationToken cancellationToken = default)
    {
        var evt = new ProductUpsertedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            IsActive = product.IsActive,
            Stock = product.Stock,
            UpdatedAt = product.UpdatedAt ?? product.CreatedAt
        };

        var payload = JsonSerializer.Serialize(evt);
        await _producer.ProduceAsync(
            _topic,
            new Message<string, string> { Key = evt.ProductId.ToString(), Value = payload },
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}

