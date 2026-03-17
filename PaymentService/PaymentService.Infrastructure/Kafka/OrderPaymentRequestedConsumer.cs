using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentService.Application.Orders.Events;
using PaymentService.Infrastructure.Idempotency;

namespace PaymentService.Infrastructure.Kafka;

public sealed class OrderPaymentRequestedConsumer : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string>? _consumer;
    private readonly IProducer<string, string>? _producer;
    private readonly string _consumeTopic;
    private readonly string _paidTopic;
    private readonly string _failedTopic;
    private CancellationTokenSource? _cts;
    private Task? _executingTask;

    public OrderPaymentRequestedConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _consumeTopic = configuration["Kafka:Topics:OrderPaymentRequested"] ?? "order-payment-requested";
        _paidTopic = configuration["Kafka:Topics:OrderPaid"] ?? "order-paid";
        _failedTopic = configuration["Kafka:Topics:OrderPaymentFailed"] ?? "order-payment-failed";

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = configuration["Kafka:Groups:PaymentOrders"] ?? "payment-orders",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All
        };

        try
        {
            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        }
        catch
        {
            _consumer = null;
        }

        try
        {
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }
        catch
        {
            _producer = null;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_consumer is null || _producer is null)
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
        if (_consumer is null || _producer is null)
            return;

        try
        {
            _consumer.Subscribe(_consumeTopic);
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

                var evt = JsonSerializer.Deserialize<OrderPaymentRequestedEvent>(cr.Message.Value);
                if (evt is null)
                    continue;

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                var eventId = $"order-payment-requested:{evt.OrderId}";
                var already = await db.ProcessedIntegrationEvents.AnyAsync(x => x.EventId == eventId, stoppingToken);
                if (already)
                    continue;

                var balance = await db.Balances.FirstOrDefaultAsync(x => x.CustomerId == evt.CustomerId, stoppingToken);
                if (balance is null)
                {
                    await PublishFailedAsync(evt, "BalanceNotFound", stoppingToken);
                    db.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent(eventId));
                    await db.SaveChangesAsync(stoppingToken);
                    continue;
                }

                if (balance.Amount < evt.TotalAmount)
                {
                    await PublishFailedAsync(evt, "InsufficientFunds", stoppingToken);
                    db.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent(eventId));
                    await db.SaveChangesAsync(stoppingToken);
                    continue;
                }

                balance.Debit(evt.TotalAmount);
                db.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent(eventId));
                await db.SaveChangesAsync(stoppingToken);

                await PublishPaidAsync(evt, stoppingToken);
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

    private async Task PublishPaidAsync(OrderPaymentRequestedEvent request, CancellationToken ct)
    {
        if (_producer is null)
            return;

        var payload = JsonSerializer.Serialize(new OrderPaidEvent
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Amount = request.TotalAmount,
            PaidAt = DateTime.UtcNow,
            Items = request.Items
                .Select(x => new OrderPaidItem { ProductId = x.ProductId, Quantity = x.Quantity })
                .ToArray()
        });

        await _producer.ProduceAsync(
            _paidTopic,
            new Message<string, string> { Key = request.OrderId.ToString(), Value = payload },
            ct);
    }

    private async Task PublishFailedAsync(OrderPaymentRequestedEvent request, string reason, CancellationToken ct)
    {
        if (_producer is null)
            return;

        var payload = JsonSerializer.Serialize(new OrderPaymentFailedEvent
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Amount = request.TotalAmount,
            Reason = reason,
            FailedAt = DateTime.UtcNow
        });

        await _producer.ProduceAsync(
            _failedTopic,
            new Message<string, string> { Key = request.OrderId.ToString(), Value = payload },
            ct);
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

        try
        {
            _producer?.Flush(TimeSpan.FromSeconds(5));
            _producer?.Dispose();
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

