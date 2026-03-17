using AutoMapper;
using CatalogService.Application.Categories.Interfaces;
using CatalogService.Application.Products.Exceptions;
using CatalogService.Application.Products.Interfaces;
using CatalogService.Domain.Entities;
using MediatR;

namespace CatalogService.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IProductEventsPublisher _eventsPublisher;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        IProductEventsPublisher eventsPublisher)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _eventsPublisher = eventsPublisher;
    }

    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.CategoryId is not null)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
            if (category is null)
                throw new CatalogValidationException($"Category '{request.CategoryId}' was not found.");
        }

        var product = _mapper.Map<Product>(request);

        await _productRepository.AddAsync(product, cancellationToken);

        await _eventsPublisher.PublishProductUpsertedAsync(product, cancellationToken);

        return _mapper.Map<ProductResponse>(product);
    }
}

