namespace StoreHub.API.DTOs;

public record AuthResponseDto(
    string Token,
    string Username,
    string Role,
    DateTime Expiration
);
