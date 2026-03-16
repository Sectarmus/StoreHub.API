using System.ComponentModel.DataAnnotations;

namespace StoreHub.API.DTOs;

public record UserRegisterDto(
    [Required] string Username,
    [Required][EmailAddress] string Email,
    [Required] string Password // Gelen şifreyi kod tarafında hashleyip DB'ye öyle göndereceğiz
);
