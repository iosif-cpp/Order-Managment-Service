using System.Text.Json;
using Confluent.Kafka;
using AuthService.Application.Auth.Events;
using AuthService.Application.Auth.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthService.Infrastructure.Kafka;

public sealed class CustomerRegisteredConsumer : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string>? _consumer;
    private readonly string _topic;
    private CancellationTokenSource? _cts;
    private Task? _executingTask;

    public CustomerRegisteredConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topics:CustomerRegistered"] ?? "customer-registered";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = configuration["Kafka:Groups:AuthCustomerRegistered"] ?? "auth-customer-registered",
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
        {
            return Task.CompletedTask;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = Task.Run(() => ExecuteAsync(_cts.Token), _cts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask is null)
        {
            return;
        }

        _cts?.Cancel();

        await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = _consumer;

        if (consumer is null)
        {
            return;
        }

        try
        {
            consumer.Subscribe(_topic);
        }
        catch
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(stoppingToken);

                if (cr is null || cr.Message is null)
                    continue;

                var evt = JsonSerializer.Deserialize<CustomerRegisteredEvent>(cr.Message.Value);
                if (evt is null)
                    continue;

                using var scope = _serviceProvider.CreateScope();
                var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

                var existing = await userRepo.GetByEmailAsync(evt.Email, stoppingToken);
                if (existing is not null)
                    continue;

                var hash = hasher.HashPassword(evt.Password);
                var user = new User(evt.Email, evt.UserName, hash);

                await userRepo.AddAsync(user, stoppingToken);
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

