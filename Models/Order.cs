namespace StoreHub.API.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }

    // --- İlişki Kısmı ---
    // Her sipariş mutlaka BİR müşteriye aittir.
    public int CustomerId { get; set; } // Foreign Key (Yabancı Anahtar)
    public Customer Customer { get; set; } = null!; // Navigation Property

    // --- Yeni Eklenen: Bir siparişte birden fazla ürün kalemi olabilir ---
    public List<OrderItem> OrderItems { get; set; } = new();
}

