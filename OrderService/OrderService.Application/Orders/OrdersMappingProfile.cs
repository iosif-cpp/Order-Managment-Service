using AutoMapper;
using OrderService.Domain.Entities;

namespace OrderService.Application.Orders;

public sealed class OrdersMappingProfile : Profile
{
    public OrdersMappingProfile()
    {
        CreateMap<Order, OrderResponse>()
            .ForMember(d => d.TotalPrice, opt => opt.MapFrom(s => s.GetTotalPrice()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
    }
}

