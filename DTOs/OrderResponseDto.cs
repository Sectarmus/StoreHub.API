namespace StoreHub.API.DTOs;

public record OrderResponseDto(
    int Id,
    int CustomerId,
    string CustomerFullName,
    DateTime OrderDate,
    decimal TotalAmount,
    List<OrderItemResponseDto> Items
);
