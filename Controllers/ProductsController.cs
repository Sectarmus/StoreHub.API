using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreHub.API.Data;
using StoreHub.API.Models;
using StoreHub.API.DTOs;
using StoreHub.API.Params;
using StoreHub.API.Helpers;

namespace StoreHub.API.Controllers;

[ApiController] // Bu sınıfın bir API Controller olduğunu belirtir.
[Route("api/[controller]")] // URL adresi: /api/products şeklinde olur.
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    // Dependency Injection: Daha önce Program.cs'te kaydettiğimiz DbContext'i buraya istiyoruz.
    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. GET: api/products (Tüm ürünleri listele)
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductResponseDto>>> GetProducts([FromQuery] ProductParams productParams)
    {
        // 1. Sorguyu oluştur (Henüz veritabanına gitmedi!)
        // Deftere Not: IQueryable, sorgunun PostgreSQL tarafına gitmeden önce hazırlandığı halidir.
        var query = _context.Products.AsQueryable();
        // 2. Filtreleme (Filtering)
        if (!string.IsNullOrEmpty(productParams.Search))
        {
            // SQL: WHERE Name ILIKE '%search%'
            query = query.Where(p => p.Name.ToLower().Contains(productParams.Search.ToLower()));
        }
        if (productParams.MinPrice.HasValue)
            query = query.Where(p => p.Price >= productParams.MinPrice.Value);
        if (productParams.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= productParams.MaxPrice.Value);
        // 3. Sayfalama (Pagination)
        // 1. Toplam sayıyı al (Filtreleme uygulandıktan sonra, ama sayfalama yapılmadan önce!)
        var totalCount = await query.CountAsync();
        // 2. Sayfalanmış veriyi çek
        // SQL: OFFSET (PageNumber-1)*PageSize LIMIT PageSize
        var products = await query
            .Skip((productParams.PageNumber - 1) * productParams.PageSize)
            .Take(productParams.PageSize)
            .ToListAsync();

        // 3. Mapping
        var productDtos = products.Select(p => new ProductResponseDto(
            p.Id, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt
        ));

        // 4. PagedResponse objesini oluştur ve dön
        var response = new PagedResponse<ProductResponseDto>(
            productDtos, totalCount, productParams.PageNumber, productParams.PageSize
        );
        return Ok(response);
    }

    // 2. POST: api/products (Ürün ekle)
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(ProductCreateDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var response = new ProductResponseDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt
        );

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, response);
    }

    // 3. GET: api/products/{id} (Tek bir ürün getir)
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"{id} numaralı ürün bulunamadı." });
        }

        var response = new ProductResponseDto(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.Stock,
        product.CreatedAt
        );

        return Ok(response);
    }

    // 4. PUT: api/products/{id} (Ürün Güncelle)
    [HttpPut("{id}")]
public async Task<IActionResult> UpdateProduct(int id, ProductUpdateDto dto)
{
    // 1. Gelen ID ile DTO içindeki ID uyuşuyor mu? (Güvenlik kontrolü)
    if (id != dto.Id)
    {
        return BadRequest(new { message = "URL'deki ID ile verideki ID eşleşmiyor." });
    }

    // 2. Veritabanında bu ürün gerçekten var mı?
    var product = await _context.Products.FindAsync(id);
    if (product == null)
    {
        return NotFound(new { message = "Güncellenecek ürün bulunamadı." });
    }

    // 3. Mapping: DTO'dan gelenleri gerçek Entity nesnesine aktar
    product.Name = dto.Name;
    product.Description = dto.Description;
    product.Price = dto.Price;
    product.Stock = dto.Stock;

    // Not: Buradan sonra 'EntityState'i elle değiştirmeye gerek yok, 
    // EF Core bu nesneyi 'Track' (takip) ettiği için değişiklikleri anlar.

    await _context.SaveChangesAsync();

    return NoContent(); // 204: Başarılı ama yeni veri dönmeye gerek yok.
}


    // 5. DELETE: api/products/{id} (Ürün Sil)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Ürün başarıyla silindi." });
    }

}
