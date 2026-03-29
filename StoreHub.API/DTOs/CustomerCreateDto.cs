namespace StoreHub.API.DTOs;

public record CustomerCreateDto(
    string FirstName,
    string LastName,
    string Email
);
