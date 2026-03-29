namespace StoreHub.API.DTOs;

public record UserRegisterDto(
    string Username,
    string Email,
    string Password
);
