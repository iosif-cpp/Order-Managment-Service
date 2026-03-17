using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderService.Application.Catalog.Events;
using OrderService.Infrastructure.ReadModels;

namespace OrderService.Infrastructure.Kafka;

public sealed class ProductUpsertedConsumer : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string>? _consumer;
    private readonly string _topic;
    private CancellationTokenSource? _cts;
    private Task? _executingTask;

    public ProductUpsertedConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topics:ProductUpserted"] ?? "product-upserted";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = configuration["Kafka:Groups:OrderCatalogProducts"] ?? "order-catalog-products",
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

                var evt = JsonSerializer.Deserialize<ProductUpsertedEvent>(cr.Message.Value);
                if (evt is null)
                    continue;

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                var existing = await db.CatalogProducts.FindAsync([evt.ProductId], stoppingToken);
                if (existing is null)
                {
                    db.CatalogProducts.Add(new CatalogProductSnapshot(
                        evt.ProductId,
                        evt.Name,
                        evt.Price,
                        evt.IsActive,
                        evt.Stock,
                        evt.UpdatedAt));
                }
                else
                {
                    existing.Update(evt.Name, evt.Price, evt.IsActive, evt.Stock, evt.UpdatedAt);
                }

                await db.SaveChangesAsync(stoppingToken);
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

