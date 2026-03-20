namespace StoreHub.API.DTOs;

public record OrderItemCreateDto(
    int ProductId,
    int Quantity
);
