using FluentValidation;
using StoreHub.API.DTOs;

namespace StoreHub.API.Validations;

public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateDtoValidator()
    {
        RuleFor(p => p.Id).GreaterThan(0).WithMessage("Geçersiz Ürün ID'si.");

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Ürün adı boş bırakılamaz!")
            .MinimumLength(2).WithMessage("Ürün adı en az 2 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Ürün adı en fazla 100 karakter olabilir.");

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("Ürün fiyatı sıfırdan büyük olmalıdır!");

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok adedi eksi (negatif) olamaz.");

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("Açıklama 500 karakteri geçemez.");
    }
}
