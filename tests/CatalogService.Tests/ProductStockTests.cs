using CatalogService.Domain.Entities;
using FluentAssertions;

namespace CatalogService.Tests;

public sealed class ProductStockTests
{
    [Fact]
    public void DecreaseStock_WhenValueIsGreaterThanStock_Throws()
    {
        var product = new Product("p", "d", 10m, "sku", null, stock: 1);

        var act = () => product.DecreaseStock(2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Not enough stock*");
    }

    [Fact]
    public void DecreaseStock_WhenValid_DecreasesStock()
    {
        var product = new Product("p", "d", 10m, "sku", null, stock: 5);

        product.DecreaseStock(3);

        product.Stock.Should().Be(2);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void IncreaseStock_WhenValueIsZeroOrNegative_Throws()
    {
        var product = new Product("p", "d", 10m, "sku", null, stock: 1);

        var act = () => product.IncreaseStock(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetStock_WhenNegative_Throws()
    {
        var product = new Product("p", "d", 10m, "sku", null, stock: 1);

        var act = () => product.SetStock(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ChangeSku_WhenWhitespace_ThrowsArgumentException()
    {
        var product = new Product("p", "d", 10m, "sku", null, stock: 1);

        var act = () => product.ChangeSku("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChangeSku_WhenValid_UpdatesSkuAndTimestamp()
    {
        var product = new Product("p", "d", 10m, "sku", null, stock: 1);

        product.ChangeSku("new-sku");

        product.Sku.Should().Be("new-sku");
        product.UpdatedAt.Should().NotBeNull();
    }
}

