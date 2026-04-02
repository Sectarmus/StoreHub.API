using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreHub.API.Data;
using StoreHub.API.DTOs;
using StoreHub.API.Models;
using StoreHub.API.Helpers;
using StoreHub.API.Params;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;

namespace StoreHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public OrdersController(AppDbContext context, IMapper mapper, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

    // GET: api/orders (List all orders - Paginated and Optimized)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResponse<OrderResponseDto>>> GetOrders([FromQuery] PaginationParams paginationParams)
    {
        string cacheKey = $"orders_{paginationParams.PageNumber}_{paginationParams.PageSize}";

        var response = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15);

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var orderDtos = _mapper.Map<List<OrderResponseDto>>(orders);

            return new PagedResponse<OrderResponseDto>(
                orderDtos, totalCount, paginationParams.PageNumber, paginationParams.PageSize
            );
        });

        return Ok(response);
    }

    // GET: api/orders/myorders (List orders for the current logged-in user)
    [HttpGet("myorders")]
    [Authorize]
    public async Task<ActionResult<List<OrderResponseDto>>> GetMyOrders()
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId))
            return Unauthorized(new { message = "Please login first." });

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var orderDtos = _mapper.Map<List<OrderResponseDto>>(orders);

        return Ok(orderDtos);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<OrderResponseDto>> CreateOrder(OrderCreateDto dto)
    {
        // Get user ID securely from JWT token
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId))
            return Unauthorized(new { message = "Please login first." });

        // Idempotency check: Prevent duplicate orders from same user within 10 seconds
        string idempotencyKey = $"last_order_{userId}";
        if (_cache.TryGetValue(idempotencyKey, out _))
            return BadRequest(new { message = "Siparişiniz zaten işleniyor veya çok yeni bir siparişiniz var. Lütfen 10 saniye bekleyin." });
            
        // Set lock for 10 seconds
        _cache.Set(idempotencyKey, true, TimeSpan.FromSeconds(10));

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0 // Ürünleri hesapladıkça üstüne ekleyeceğiz
            };

            foreach (var itemDto in dto.Items)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                    return NotFound(new { message = $"Product with ID {itemDto.ProductId} not found." });

                if (product.Stock < itemDto.Quantity)
                    return BadRequest(new { message = $"Not enough stock for {product.Name}. Remaining: {product.Stock}" });

                product.Stock -= itemDto.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price
                };

                order.TotalAmount += (orderItem.Quantity * orderItem.UnitPrice);

                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = _mapper.Map<OrderResponseDto>(order);

            // Invalidate cache to update lists
            if (_cache is Microsoft.Extensions.Caching.Memory.MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, response);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponseDto>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        
        var orderDto = _mapper.Map<OrderResponseDto>(order);

        return Ok(orderDto);
    }
}
