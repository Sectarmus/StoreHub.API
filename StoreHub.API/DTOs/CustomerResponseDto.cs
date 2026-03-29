namespace StoreHub.API.DTOs;

public record CustomerResponseDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string FullName
);
