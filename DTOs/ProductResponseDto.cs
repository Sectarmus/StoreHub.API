namespace StoreHub.API.DTOs;

public record ProductResponseDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt
);
