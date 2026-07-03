using FluentValidation;
using Meridian.Bll.Dtos;

namespace Meridian.Bll.Validators;

public class PublishCardRequestValidator : AbstractValidator<PublishCardRequestDto>
{
    private static readonly string[] PublishableTypes = { "Safety", "Resource" };

    public PublishCardRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Type)
            .Must(t => PublishableTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Type must be Safety or Resource — contacts are not broadcast.");
        RuleFor(x => x.Url).MaximumLength(2048);
    }
}
