using AutoMapper;
using CatalogService.Application.Products.Interfaces;
using MediatR;

namespace CatalogService.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyCollection<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<ProductResponse>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);

        return products
            .Select(p => _mapper.Map<ProductResponse>(p))
            .ToArray();
    }
}

