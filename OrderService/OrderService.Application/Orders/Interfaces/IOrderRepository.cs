using OrderService.Domain.Entities;

namespace OrderService.Application.Orders.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Order>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

