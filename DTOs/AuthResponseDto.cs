namespace StoreHub.API.DTOs;

public record AuthResponseDto(
    string Token,        // Kimlik Kartı (JWT)
    string Username,
    string Role,
    DateTime Expiration  // Kartın süresi
);
