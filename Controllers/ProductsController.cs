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

    // 3. GET: api/products/{id} (Tek bir ürün getir)
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"{id} numaralı ürün bulunamadı." });
        }

        return product;
    }

    // 4. PUT: api/products/{id} (Ürün Güncelle)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest(new { message = "ID eşleşmiyor." });
        }

        _context.Entry(product).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Products.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent(); // 204 başarı, ama döndürülecek veri yok.
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
