using FluentValidation;
using StoreHub.API.DTOs;

namespace StoreHub.API.Validations;

public class OrderItemCreateDtoValidator : AbstractValidator<OrderItemCreateDto>
{
    public OrderItemCreateDtoValidator()
    {
        RuleFor(oi => oi.ProductId)
            .GreaterThan(0).WithMessage("A valid product must be selected.");

        RuleFor(oi => oi.Quantity)
            .GreaterThan(0).WithMessage("Order quantity must be at least 1.");
    }
}

public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator()
    {
        RuleFor(o => o.Items)
            .NotEmpty().WithMessage("Your order must contain at least one product.");

        RuleForEach(o => o.Items).SetValidator(new OrderItemCreateDtoValidator());
    }
}
