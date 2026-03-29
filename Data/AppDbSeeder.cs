using StoreHub.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json; // JSON asistanımız
using BCrypt.Net;

namespace StoreHub.API.Data;

public static class AppDbSeeder
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task SeedAsync(AppDbContext context)
    {
        // Seed users (Admin and Customer)
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@storehub.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin"
            };

            var customerUser = new User
            {
                Username = "test_customer",
                Email = "customer@storehub.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
                Role = "Customer"
            };

            context.Users.AddRange(adminUser, customerUser);
            await context.SaveChangesAsync();
        }

        // Seed products using DummyJSON API
        var hasCategories = await context.Products.AnyAsync(p => p.Category != "Genel" && p.Category != "");
        if (!hasCategories || await context.Products.CountAsync() < 10)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<DummyJsonResponse>("https://dummyjson.com/products?limit=50");

                if (response != null && response.Products != null)
                {
                    // Clear existing dummy products to refresh data
                    var existingProducts = await context.Products.ToListAsync();
                    context.Products.RemoveRange(existingProducts);
                    await context.SaveChangesAsync();

                    foreach (var p in response.Products)
                    {
                        var product = new Product
                        {
                            Name = p.Title,
                            Description = p.Description,
                            Price = p.Price,
                            Stock = p.Stock,
                            Category = p.Category,
                            ImageUrl = p.Thumbnail
                        };
                        context.Products.Add(product);
                    }
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seeder Error: {ex.Message}");
            }
        }
    }
}

// Helper classes for DummyJSON API
public class DummyJsonResponse
{
    public List<DummyProduct>? Products { get; set; }
}

public class DummyProduct
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = "";
    public string Thumbnail { get; set; } = "";
}
