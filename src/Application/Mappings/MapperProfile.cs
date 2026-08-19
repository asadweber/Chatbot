using Application.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();

        CreateMap<Customer, CustomerDto>().ReverseMap();

        CreateMap<OrderDetail, OrderDetailDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ReverseMap()
            .ForMember(d => d.Product, o => o.Ignore());

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.Name))
            .ReverseMap()
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.TotalAmount, o => o.Ignore())
            .ForMember(d => d.Customer, o => o.Ignore());

        CreateMap<Document, DocumentDto>()
            .ForMember(d => d.ChunkCount, o => o.MapFrom(s => s.Chunks.Count));

        CreateMap<ChatSession, ChatSessionDto>();
        CreateMap<ChatMessage, ChatMessageDto>();

    }
}
