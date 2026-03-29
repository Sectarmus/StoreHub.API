namespace StoreHub.API.DTOs;

public record OrderCreateDto(
    List<OrderItemCreateDto> Items
);
