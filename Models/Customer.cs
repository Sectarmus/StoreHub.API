namespace StoreHub.API.Models;

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [EmailAddress] // E-posta formatını otomatik kontrol eder.
    public string Email { get; set; } = string.Empty;

    // --- İlişki Kısmı (Navigation Property) ---
    // Deftere Not: Bir müşterinin birden fazla siparişi olabilir.
    public List<Order> Orders { get; set; } = new(); 
}
