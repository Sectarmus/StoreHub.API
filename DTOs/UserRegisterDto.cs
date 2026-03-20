namespace StoreHub.API.DTOs;

public record UserRegisterDto(
    string Username,
    string Email,
    string Password // Gelen şifreyi kod tarafında hashleyip DB'ye öyle göndereceğiz
);
