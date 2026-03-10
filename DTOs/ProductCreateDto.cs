using System.ComponentModel.DataAnnotations;

namespace StoreHub.API.DTOs;

public record ProductCreateDto(
    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [MaxLength(100)]
    string Name,

    string Description,

    [Range(0.01, double.MaxValue)]
    decimal Price,

    [Range(0, int.MaxValue)]
    int Stock
);
