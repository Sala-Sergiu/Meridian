using FluentValidation;
using Meridian.Bll.Dtos;

namespace Meridian.Bll.Validators;

// Validation rules for the login request.
public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
