using AutoMapper;
using CatalogService.API.Controllers;
using CatalogService.Application.Products.Commands.CreateProduct;
using CatalogService.Application.Products.Commands.UpdateProduct;

namespace CatalogService.API.Mapping;

public sealed class ProductApiMappingProfile : Profile
{
    public ProductApiMappingProfile()
    {
        CreateMap<CreateProductRequest, CreateProductCommand>();

        CreateMap<UpdateProductRequest, UpdateProductCommand>()
            .ForMember(d => d.Id, opt => opt.Ignore());
    }
}

