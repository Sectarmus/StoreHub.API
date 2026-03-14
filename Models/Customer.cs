using System.ComponentModel.DataAnnotations;

namespace StoreHub.API.Models;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress] // E-posta formatını otomatik kontrol eder.
    public string Email { get; set; } = string.Empty;

    // --- İlişki Kısmı (Navigation Property) ---
    // Deftere Not: Bir müşterinin birden fazla siparişi olabilir.
    public List<Order> Orders { get; set; } = new(); 
}
