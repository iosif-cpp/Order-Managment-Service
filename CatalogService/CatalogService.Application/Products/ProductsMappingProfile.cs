using AutoMapper;
using CatalogService.Application.Products.Commands.CreateProduct;
using CatalogService.Application.Products.Commands.UpdateProduct;
using CatalogService.Domain.Entities;

namespace CatalogService.Application.Products;

public sealed class ProductsMappingProfile : Profile
{
    public ProductsMappingProfile()
    {
        CreateMap<Product, ProductResponse>();

        CreateMap<CreateProductCommand, Product>()
            .ConstructUsing(c => new Product(
                c.Name,
                c.Description,
                c.Price,
                c.Sku,
                c.CategoryId,
                c.Stock));

        CreateMap<UpdateProductCommand, Product>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.IsActive, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore());
    }
}

