namespace StoreHub.API.Models;

public class OrderItem
{
    public int Id { get; set; }
    
    public int OrderId { get; set; } // Hangi faturaya ait?
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; } // Hangi ürün satılmış?
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; } // Kaç adet?
    public decimal UnitPrice { get; set; } // Satıldığı andaki fiyatı (Çünkü ürünün fiyatı yarın değişebilir, ama faturadaki sabit kalmalı!)
}
