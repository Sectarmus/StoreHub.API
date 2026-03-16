using Microsoft.EntityFrameworkCore;
using StoreHub.API.Models;

namespace StoreHub.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Bu satır "Ürünlerimi veritabanında bir tablo yap" demektir.
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<User> Users { get; set; } // Yetkili ve kullanıcılara ait tablo
}

