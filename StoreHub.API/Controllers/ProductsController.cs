using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreHub.API.Data;
using StoreHub.API.Models;
using StoreHub.API.DTOs;
using StoreHub.API.Params;
using StoreHub.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using System.IO;
using Microsoft.Extensions.Caching.Memory; // IMemoryCache kütüphanesi

namespace StoreHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public ProductsController(AppDbContext context, IMapper mapper, IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

    // 1. GET: api/products (List all products)
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductResponseDto>>> GetProducts([FromQuery] ProductParams productParams)
    {
        string cacheKey = $"products_{productParams.PageNumber}_{productParams.PageSize}_{productParams.Search}_{productParams.MinPrice}_{productParams.MaxPrice}_{productParams.Category}";

        bool skipCache = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");

        if (skipCache || !_cache.TryGetValue(cacheKey, out PagedResponse<ProductResponseDto>? response))
        {
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(productParams.Search))
            {
                var search = productParams.Search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search) || p.Description.ToLower().Contains(search));
            }
            if (productParams.MinPrice.HasValue)
                query = query.Where(p => p.Price >= productParams.MinPrice.Value);
            if (productParams.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= productParams.MaxPrice.Value);
            if (!string.IsNullOrEmpty(productParams.Category))
                query = query.Where(p => p.Category.ToLower() == productParams.Category.ToLower());
                
            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((productParams.PageNumber - 1) * productParams.PageSize)
                .Take(productParams.PageSize)
                .ToListAsync();

            var productDtos = _mapper.Map<List<ProductResponseDto>>(products);

            response = new PagedResponse<ProductResponseDto>(
                productDtos, totalCount, productParams.PageNumber, productParams.PageSize, fromCache: false
            );

            if (!skipCache)
            {
                _cache.Set(cacheKey, response, TimeSpan.FromMinutes(10));
            }
        }
        else
        {
            response!.FromCache = true;
        }

        return Ok(response);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<string>>> GetCategories()
    {
        var categories = await _cache.GetOrCreateAsync("categories_list", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .Where(c => !string.IsNullOrEmpty(c))
                .ToListAsync();
        });

        return Ok(categories);
    }

    // 2. POST: api/products (Add a product) - Admins only
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(ProductCreateDto dto)
    {
        var product = _mapper.Map<Product>(dto);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<ProductResponseDto>(product);

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, response);
    }

    // 3. GET: api/products/{id} (Get single product)
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        var response = _mapper.Map<ProductResponseDto>(product);

        return Ok(response);
    }

    // 4. PUT: api/products/{id} (Update product) - Admins only
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(int id, ProductUpdateDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in data." });
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Product to update not found." });
        }

        _mapper.Map(dto, product);

        await _context.SaveChangesAsync();

        return NoContent(); // 204: Success, no content to return
    }


    // 5. DELETE: api/products/{id} (Delete product) - Admins only
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Product deleted successfully." });
    }

    // 6. POST: api/products/{id}/image (Upload image) - Admins only
    [HttpPost("{id}/image")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadProductImage(int id, IFormFile file)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound("Product not found");

        if (file == null || file.Length == 0) return BadRequest("Please select a file.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension)) return BadRequest("Only image files (jpg, png) are accepted!");

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        var fileName = Guid.NewGuid().ToString() + extension;
        var fullPath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        product.ImageUrl = $"/images/products/{fileName}";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Image uploaded successfully", url = product.ImageUrl });
    }
}
