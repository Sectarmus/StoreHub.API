namespace StoreHub.API.DTOs;

public record CustomerUpdateDto(
    int Id,
    string FirstName,
    string LastName,
    string Email
);
