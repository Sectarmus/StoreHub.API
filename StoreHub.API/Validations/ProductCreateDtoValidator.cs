using FluentValidation;
using StoreHub.API.DTOs;

namespace StoreHub.API.Validations;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name cannot be empty.")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Product name can be a maximum of 100 characters.");

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("Product price must be greater than zero.");

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be a negative value.");

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("Product description can be a maximum of 500 characters.");
    }
}
