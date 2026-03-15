using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreHub.API.Data;
using StoreHub.API.DTOs;
using StoreHub.API.Models;

namespace StoreHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> CreateOrder(OrderCreateDto dto)
    {
        // 1. Müşteri var mı kontrol kontrolü
        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
            return NotFound(new { message = "Müşteri bulunamadı." });

        // İş İşlemleri (Transaction) Başlat
        // Deftere Not: Veritabanında bir işlem yarım kalmasın diye "Transaction" kullanırız. 
        // Ya hepsi başarılı olur ya da hiçbiri kaydedilmez.
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0 // Ürünleri hesapladıkça üstüne ekleyeceğiz
            };

            foreach (var itemDto in dto.Items)
            {
                // Ürün stokta var mı kontrol edelim
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                    return NotFound(new { message = $"{itemDto.ProductId} ID'li ürün bulunamadı." });

                if (product.Stock < itemDto.Quantity)
                    return BadRequest(new { message = $"{product.Name} ürününden stokta yeterli yok. Kalan: {product.Stock}" });

                // Ürün satıldığı için stoğu DÜŞÜRÜYORUZ!
                product.Stock -= itemDto.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price // Tam o andaki satış fiyatını donduruyoruz
                };

                // Fatura toplam tutarını hesaplıyoruz
                order.TotalAmount += (orderItem.Quantity * orderItem.UnitPrice);

                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Eğer kod buraya ulaştıysa hata çıkmamıştır, işlemleri kalıcı yap.
            await transaction.CommitAsync();

            // Sadece örnek olsun diye basit bir Response dönüyoruz, 
            // gerçekte Include() yapıp ilişkili verileri çekerek daha zengin Response yapabilirsin.
            var response = new OrderResponseDto(
                order.Id,
                order.CustomerId,
                $"{customer.FirstName} {customer.LastName}",
                order.OrderDate,
                order.TotalAmount,
                order.OrderItems.Select(oi => new OrderItemResponseDto(
                    oi.ProductId,
                    "Ürün", // Optimizasyon için geçici sabit isim
                    oi.Quantity,

                    oi.UnitPrice,
                    oi.Quantity * oi.UnitPrice
                )).ToList()
            );

            // Burada da 201 Created döndürüyoruz, tıpkı geçen derste öğrendiğimiz gibi.
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, response);
        }
        catch (Exception) // Hata olursa catch'e düşer
        {
            await transaction.RollbackAsync(); // Hiçbir işlemi kaydetme, geri al!
            throw; // Hatayı ExceptionMiddleware'e fırlat ki JSON mesajına çevirsin
        }
    }

    // Basit bir GET metodu ki CreatedAtAction kırılmasın.
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponseDto>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Zincirleme Include! (Satır -> Ürün bilgileri)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        var response = new OrderResponseDto(
            order.Id,
            order.CustomerId,
            $"{order.Customer.FirstName} {order.Customer.LastName}",
            order.OrderDate,
            order.TotalAmount,
            order.OrderItems.Select(oi => new OrderItemResponseDto(
                oi.ProductId,
                oi.Product.Name,
                oi.Quantity,
                oi.UnitPrice,
                oi.Quantity * oi.UnitPrice
            )).ToList()
        );

        return Ok(response);
    }
}
