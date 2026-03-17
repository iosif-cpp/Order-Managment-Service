using System;

namespace OrderService.Domain.Entities;

public sealed class OrderItem
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = null!;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public OrderItem(Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void IncreaseQuantity(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        Quantity += value;
    }

    public void DecreaseQuantity(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        if (value > Quantity)
            throw new InvalidOperationException("Cannot decrease quantity below zero.");

        Quantity -= value;
    }
}

