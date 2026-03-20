namespace StoreHub.API.DTOs;

public record UserLoginDto(
    [Required] string Username,
    [Required] string Password
);
