namespace StoreHub.API.DTOs;

public record ProductCreateDto(
    string Name,

    string Description,
    decimal Price,
    int Stock
);
