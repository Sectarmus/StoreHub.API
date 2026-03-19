using AutoMapper;
using StoreHub.API.Models;
using StoreHub.API.DTOs;

namespace StoreHub.API.Mappings;

// AutoMapper'ın ayar dosyası olması için 'Profile' sınıfından miras alması (kalıtım) ŞARTTIR.
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // 1. Müşteri (Customer) Dönüşümleri

        // DTO'dan -> Veritabanı Modeline (Kullanıcı Form doldurup gönderdiğinde kullanılır)
        CreateMap<CustomerCreateDto, Customer>();
        CreateMap<CustomerUpdateDto, Customer>();

        // Veritabanı Modelinden -> DTO'ya (Veritabanındaki kaydı dışarı açarken kullanılır)
        CreateMap<Customer, CustomerResponseDto>()
            // SİHİR BURADA: FullName adında DTO'da bir parametremiz vardı ama Customer tablosunda yoktu.
            // Onu da tek satırda AutoMapper'a öğretiyoruz; git FirstName ve LastName'i birleştir oraya koy:
            .ForCtorParam("FullName", opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        
        // Ödev (Çok Kolay): Aynı şekilde ProductDtoleri ve Product Modeli için CreateMap'leri
        // sen de buranın altına ekleyebilirsin:
        CreateMap<ProductCreateDto, Product>();
        CreateMap<ProductUpdateDto, Product>();
        CreateMap<Product, ProductResponseDto>();
    }
}
