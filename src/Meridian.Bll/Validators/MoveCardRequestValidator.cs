using FluentValidation;
using Meridian.Bll.Dtos;
using Meridian.Domain.Enums;

namespace Meridian.Bll.Validators;

// Validation rules for the move-card request. Invalid input becomes a 400
// ProblemDetails at the API edge.
public class MoveCardRequestValidator : AbstractValidator<MoveCardRequestDto>
{
    public MoveCardRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .IsEnumName(typeof(CardStatus), caseSensitive: false)
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<CardStatus>())}.");
    }
}
