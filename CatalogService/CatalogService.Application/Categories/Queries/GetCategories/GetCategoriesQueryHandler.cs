using AutoMapper;
using CatalogService.Application.Categories.Interfaces;
using MediatR;

namespace CatalogService.Application.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyCollection<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<CategoryResponse>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return categories
            .Select(c => _mapper.Map<CategoryResponse>(c))
            .ToArray();
    }
}

