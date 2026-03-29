namespace StoreHub.API.DTOs;

public record OrderResponseDto(
    int Id,
    int UserId,
    string UserName,
    DateTime OrderDate,
    decimal TotalAmount,
    List<OrderItemResponseDto> Items
);
