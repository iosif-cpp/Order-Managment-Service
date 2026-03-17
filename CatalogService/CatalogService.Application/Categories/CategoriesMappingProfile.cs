using AutoMapper;
using CatalogService.Domain.Entities;

namespace CatalogService.Application.Categories;

public sealed class CategoriesMappingProfile : Profile
{
    public CategoriesMappingProfile()
    {
        CreateMap<Category, CategoryResponse>();
    }
}

