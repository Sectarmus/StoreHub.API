namespace StoreHub.API.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public string ImageUrl { get; set; } = string.Empty; // Resmin sunucudaki yolu (Örn: "images/products/mouse.jpg")

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
