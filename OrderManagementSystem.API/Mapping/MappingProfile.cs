using AutoMapper;
using OrderManagementSystem.API.Models;
using OrderManagementSystem.API.DTOs;

namespace OrderManagementSystem.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.DiscountPercent, opt => opt.MapFrom(src => src.DiscountPercentage))
                .ForMember(dest => dest.DiscountQuantityThreshold, opt => opt.MapFrom(src => src.DiscountQuantityThreshold ?? 0));
        }
    }
}

