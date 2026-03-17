using AutoMapper;
using CatalogService.Application.Categories.Interfaces;
using CatalogService.Domain.Entities;
using MediatR;

namespace CatalogService.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category(request.Name, request.Description);

        await _categoryRepository.AddAsync(category, cancellationToken);

        return _mapper.Map<CategoryResponse>(category);
    }
}

