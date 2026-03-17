using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentService.Application.Customers.Events;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Idempotency;

namespace PaymentService.Infrastructure.Kafka;

public sealed class CustomerRegisteredConsumer : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string>? _consumer;
    private readonly string _topic;
    private CancellationTokenSource? _cts;
    private Task? _executingTask;

    public CustomerRegisteredConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topics:CustomerRegistered"] ?? "customer-registered";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = configuration["Kafka:Groups:PaymentCustomers"] ?? "payment-customers",
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

                var evt = JsonSerializer.Deserialize<CustomerRegisteredEvent>(cr.Message.Value);
                if (evt is null)
                    continue;

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                var eventId = evt.UserId.ToString();
                var already = await db.ProcessedIntegrationEvents.AnyAsync(x => x.EventId == eventId, stoppingToken);
                if (already)
                    continue;

                var existing = await db.Balances.FindAsync([evt.UserId], stoppingToken);
                if (existing is null)
                {
                    db.Balances.Add(new Balance(evt.UserId, 0m));
                }

                db.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent(eventId));
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

