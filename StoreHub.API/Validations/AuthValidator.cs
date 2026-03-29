using FluentValidation;
using StoreHub.API.DTOs;

namespace StoreHub.API.Validations;

public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterDtoValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Kullanıcı adı boş olamaz.")
            .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır.")
            .MaximumLength(50).WithMessage("Kullanıcı adı en fazla 50 karakter olabilir.");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Lütfen geçerli bir e-posta formatı giriniz.");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MinimumLength(6).WithMessage("Şifre güvenliğiniz için en az 6 karakter olmalıdır.");
    }
}

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Kullanıcı adı boş bırakılamaz.");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Şifre boş bırakılamaz.");
    }
}
