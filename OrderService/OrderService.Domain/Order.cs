using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderService.Domain.Entities;

public sealed class Order
{
    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order()
    {
    }

    public Order(Guid customerId)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Status = OrderStatus.PendingPayment;
        CreatedAt = DateTime.UtcNow;
    }

    public decimal GetTotalPrice()
    {
        return _items.Sum(i => i.UnitPrice * i.Quantity);
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == productId && i.UnitPrice == unitPrice);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            var item = new OrderItem(Id, productId, productName, unitPrice, quantity);
            _items.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            return;

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            return;

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
    }
}

