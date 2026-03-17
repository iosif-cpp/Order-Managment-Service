using CatalogService.Application.Products.Interfaces;
using MediatR;

namespace CatalogService.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductEventsPublisher _eventsPublisher;

    public UpdateProductCommandHandler(IProductRepository productRepository, IProductEventsPublisher eventsPublisher)
    {
        _productRepository = productRepository;
        _eventsPublisher = eventsPublisher;
    }

    public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            throw new KeyNotFoundException($"Product with id '{request.Id}' was not found.");

        product.Rename(request.Name);
        product.ChangeDescription(request.Description);
        product.ChangePrice(request.Price);
        product.ChangeSku(request.Sku);
        product.ChangeCategory(request.CategoryId);
        product.SetStock(request.Stock);

        await _productRepository.SaveChangesAsync(cancellationToken);

        await _eventsPublisher.PublishProductUpsertedAsync(product, cancellationToken);

        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Sku,
            product.IsActive,
            product.Stock,
            product.CategoryId);
    }
}

