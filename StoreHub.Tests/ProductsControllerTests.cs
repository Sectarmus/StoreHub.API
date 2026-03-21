using StoreHub.API.Controllers;
using StoreHub.API.Data;
using StoreHub.API.Models;
using StoreHub.API.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq; // Sisteme Moq kütüphanesini tanıttık
using Xunit; // Test Kütüphanesi

namespace StoreHub.Tests;

public class ProductsControllerTests
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ProductsController _controller;

    // Hazırlık: Her test çalışmadan önce "Sanal" bir ortam oluşturulur (Arrange Katmanı)
    public ProductsControllerTests()
    {
        // 1. Sanal (InMemory) Veritabanı Kurulumu:
        // C# RAM üzerinde geçici bir DB açar, test bittiğinde hepsi uçup gider, gerçek PostgreSQL'e dokunmaz!
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        // 2. Sanal (Mock) Mapper Kurulumu:
        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(m => m.Map<ProductResponseDto>(It.IsAny<Product>()))
            .Returns((Product src) => new ProductResponseDto(
                src.Id, src.Name, src.Description, src.Price, src.Stock, src.ImageUrl, src.CreatedAt
            ));
        _mapper = mockMapper.Object;

        // 3. Controller'ın Test Örneğini (Instance) Yaratma
        _controller = new ProductsController(_context, _mapper);
    }

    // 1. TEST METODU
    [Fact] // XUnit için basit bir test senaryosu olduğunu belirtir
    public async Task GetProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Act (Eylem) - Sahte veritabanında "999" numaralı ürünü bulmaya çalış
        var result = await _controller.GetProduct(999);
        
        // Assert (Doğrulama) - Sistem gerçekten de "NotFound (404)" hatası fırlatmalı!
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // 2. TEST METODU
    [Fact]
    public async Task GetProduct_ShouldReturnProduct_WhenProductExists()
    {
        // Tahsisat (Arrange) - Sanal veritabanına bir yalan ürün ekleyelim
        var product = new Product { Name = "Test Klavye", Price = 500, Stock = 10, Description = "Sanal Ürün" };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Eylem (Act) - Controller üzerinden az önce eklediğimiz ürünü kendi ID'si ile çekmek isteyelim
        var result = await _controller.GetProduct(product.Id);

        // Doğrulama (Assert)
        // 1. Başarılı (200 OK) döndüğünü doğrula
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        // 2. Dönen tipin ProductResponseDto olduğunu doğrula
        var returnedProduct = Assert.IsType<ProductResponseDto>(okResult.Value);
        // 3. Gelen adın bizim eklediğimiz ada eşit olduğunu doğrula
        Assert.Equal("Test Klavye", returnedProduct.Name);
        Assert.Equal(500, returnedProduct.Price);
    }
}
