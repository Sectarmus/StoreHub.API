using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreHub.API.Data;
using StoreHub.API.Models;

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
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    // 2. POST: api/products (Ürün ekle)
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
    }
}
