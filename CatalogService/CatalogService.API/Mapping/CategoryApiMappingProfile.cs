using AutoMapper;
using CatalogService.API.Controllers;
using CatalogService.Application.Categories.Commands.CreateCategory;

namespace CatalogService.API.Mapping;

public sealed class CategoryApiMappingProfile : Profile
{
    public CategoryApiMappingProfile()
    {
        CreateMap<CreateCategoryRequest, CreateCategoryCommand>();
    }
}

