using System.Text.Json;
using CatalogService.Application.Orders.Events;
using CatalogService.Application.Products.Interfaces;
using CatalogService.Infrastructure.Idempotency;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CatalogService.Infrastructure.Kafka;

public sealed class OrderPaidConsumer : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string>? _consumer;
    private readonly string _topic;
    private CancellationTokenSource? _cts;
    private Task? _executingTask;

    public OrderPaidConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topics:OrderPaid"] ?? "order-paid";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = configuration["Kafka:Groups:CatalogOrders"] ?? "catalog-orders",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        try
        {
            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }
        catch
        {
            _consumer = null;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_consumer is null)
            return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = Task.Run(() => ExecuteAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask is null)
            return;

        _cts?.Cancel();
        await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_consumer is null)
            return;

        try
        {
            _consumer.Subscribe(_topic);
        }
        catch
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cr = _consumer.Consume(stoppingToken);
                if (cr?.Message?.Value is null)
                    continue;

                var evt = JsonSerializer.Deserialize<OrderPaidEvent>(cr.Message.Value);
                if (evt is null)
                    continue;

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IProductEventsPublisher>();

                var eventId = $"order-paid:{evt.OrderId}";
                var already = await db.ProcessedIntegrationEvents.AnyAsync(x => x.EventId == eventId, stoppingToken);
                if (already)
                    continue;

                var ids = evt.Items.Select(i => i.ProductId).Distinct().ToArray();
                var products = await db.Products.Where(p => ids.Contains(p.Id)).ToListAsync(stoppingToken);

                if (products.Count != ids.Length)
                    continue;

                foreach (var item in evt.Items)
                {
                    var product = products.First(p => p.Id == item.ProductId);
                    if (!product.IsActive)
                        throw new InvalidOperationException("Product is not active.");

                    product.DecreaseStock(item.Quantity);
                }

                db.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent(eventId));
                await db.SaveChangesAsync(stoppingToken);

                foreach (var product in products)
                {
                    await publisher.PublishProductUpsertedAsync(product, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
            }
        }
    }

    public void Dispose()
    {
        try
        {
            _consumer?.Close();
            _consumer?.Dispose();
        }
        catch
        {
        }
        finally
        {
            _cts?.Dispose();
        }
    }
}

