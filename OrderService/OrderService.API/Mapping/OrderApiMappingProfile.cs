using AutoMapper;
using OrderService.API.Controllers;
using OrderService.Application.Orders.Commands.CreateOrder;

namespace OrderService.API.Mapping;

public sealed class OrderApiMappingProfile : Profile
{
    public OrderApiMappingProfile()
    {
        CreateMap<CreateOrderRequest, CreateOrderCommand>()
            .ConstructUsing((src, ctx) => new CreateOrderCommand(Guid.Empty, src.Items))
            .ForMember(d => d.CustomerId, opt => opt.Ignore());
    }
}

