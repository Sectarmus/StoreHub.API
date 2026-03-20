namespace StoreHub.API.DTOs;

public record OrderCreateDto(
    int CustomerId,
    List<OrderItemCreateDto> Items
);
