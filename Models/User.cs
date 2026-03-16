using System.ComponentModel.DataAnnotations;

namespace StoreHub.API.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty; // Şifrelenmiş halde tutacağız!

    public string Role { get; set; } = "Customer"; // Sisteme giren varsayılan olarak müşteridir. ("Admin" yapacağız bazılarını)
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
