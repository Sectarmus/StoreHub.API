using FluentValidation;
using StoreHub.API.DTOs;

namespace StoreHub.API.Validations;

public class CustomerCreateDtoValidator : AbstractValidator<CustomerCreateDto>
{
    public CustomerCreateDtoValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty().WithMessage("Müşteri adı boş olamaz.")
            .MaximumLength(50).WithMessage("Müşteri adı en fazla 50 karakter olabilir.");

        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("Müşteri soyadı boş olamaz.")
            .MaximumLength(50).WithMessage("Müşteri soyadı en fazla 50 karakter olabilir.");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("E-posta alanı zorunludur.")
            .EmailAddress().WithMessage("Lütfen geçerli bir e-posta adresi giriniz.");
    }
}

public class CustomerUpdateDtoValidator : AbstractValidator<CustomerUpdateDto>
{
    public CustomerUpdateDtoValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0).WithMessage("Geçersiz Müşteri ID'si.");

        RuleFor(c => c.FirstName)
            .NotEmpty().WithMessage("Müşteri adı boş olamaz.")
            .MaximumLength(50).WithMessage("Müşteri adı en fazla 50 karakter olabilir.");

        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("Müşteri soyadı boş olamaz.")
            .MaximumLength(50).WithMessage("Müşteri soyadı en fazla 50 karakter olabilir.");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("E-posta alanı zorunludur.")
            .EmailAddress().WithMessage("Lütfen geçerli bir e-posta adresi giriniz.");
    }
}
