using System.ComponentModel.DataAnnotations;

namespace StoreHub.API.DTOs;

public record OrderCreateDto(
    [Required] int CustomerId,
    [Required][MinLength(1, ErrorMessage = "Siparişte en az bir ürün olmalıdır.")] List<OrderItemCreateDto> Items
);
