using System.ComponentModel.DataAnnotations;

namespace StoreHub.API.DTOs;

public record OrderItemCreateDto(
    [Required] int ProductId,
    [Required][Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")] int Quantity
);
