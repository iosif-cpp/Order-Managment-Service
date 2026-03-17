using AutoMapper;
using CatalogService.Application.Products.Interfaces;
using MediatR;

namespace CatalogService.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponse?>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<ProductResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return null;

        return _mapper.Map<ProductResponse>(product);
    }
}

