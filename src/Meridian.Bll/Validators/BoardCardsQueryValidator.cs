using FluentValidation;
using Meridian.Bll.Dtos;
using Meridian.Bll.QueryPipeline;
using Meridian.Domain.Enums;

namespace Meridian.Bll.Validators;

// Validation rules for the board-cards query parameters. Invalid input becomes
// a 400 ProblemDetails at the API edge, before the pipeline is composed.
public class BoardCardsQueryValidator : AbstractValidator<BoardCardsQueryDto>
{
    private const int MaxPageSize = 100;

    public BoardCardsQueryValidator()
    {
        RuleFor(x => x.Status)
            .IsEnumName(typeof(CardStatus), caseSensitive: false)
            .When(x => x.Status is not null)
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<CardStatus>())}.");

        RuleFor(x => x.Type)
            .IsEnumName(typeof(CardType), caseSensitive: false)
            .When(x => x.Type is not null)
            .WithMessage($"Type must be one of: {string.Join(", ", Enum.GetNames<CardType>())}.");

        RuleFor(x => x.Sort)
            .Must(sort => string.Equals(sort, "asc", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(sort, BoardCardsPipeline.SortDescending, StringComparison.OrdinalIgnoreCase))
            .When(x => x.Sort is not null)
            .WithMessage("Sort must be 'asc' or 'desc'.");

        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
