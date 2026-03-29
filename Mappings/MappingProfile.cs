using AutoMapper;
using StoreHub.API.Models;
using StoreHub.API.DTOs;

namespace StoreHub.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ProductCreateDto, Product>();
        CreateMap<ProductUpdateDto, Product>();
        CreateMap<Product, ProductResponseDto>();

        CreateMap<OrderItem, OrderItemResponseDto>()
            .ForCtorParam("ProductName", opt => opt.MapFrom(src => src.Product.Name))
            .ForCtorParam("TotalPrice", opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));

        CreateMap<Order, OrderResponseDto>()
            .ForCtorParam("UserName", opt => opt.MapFrom(src => src.User.Username))
            .ForCtorParam("Items", opt => opt.MapFrom(src => src.OrderItems));
    }
}
