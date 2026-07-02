using Meridian.Bll.Dtos;
using Meridian.Bll.QueryPipeline.Steps;
using Meridian.Domain.Common;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;

namespace Meridian.Bll.QueryPipeline;

// Assembles the query pipeline for a board's cards from the request
// parameters, in a fixed sensible order: filters -> sort -> paging.
// Unparsable filter values are simply skipped — the API validator has already
// rejected genuinely invalid input with a 400.
public static class BoardCardsPipeline
{
    public const string SortDescending = "desc";

    public static IReadOnlyList<IQueryStep<BoardCard>> Compose(BoardCardsQueryDto query)
    {
        var steps = new List<IQueryStep<BoardCard>>();

        if (Enum.TryParse<CardStatus>(query.Status, ignoreCase: true, out var status))
        {
            steps.Add(new CardStatusFilterStep(status));
        }

        if (Enum.TryParse<CardType>(query.Type, ignoreCase: true, out var type))
        {
            steps.Add(new CardTypeFilterStep(type));
        }

        var descending = string.Equals(query.Sort, SortDescending, StringComparison.OrdinalIgnoreCase);
        steps.Add(new OrderSortStep(descending));

        steps.Add(new PagingStep<BoardCard>(query.Page, query.PageSize));

        return steps;
    }
}
