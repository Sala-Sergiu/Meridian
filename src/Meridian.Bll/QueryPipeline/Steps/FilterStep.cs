using System.Linq.Expressions;
using Meridian.Domain.Common;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;

namespace Meridian.Bll.QueryPipeline.Steps;

// Filtering step (Where). Standard LINQ only — no EF Core APIs.
public class FilterStep<T> : IQueryStep<T>
{
    private readonly Expression<Func<T, bool>> _predicate;

    public FilterStep(Expression<Func<T, bool>> predicate)
    {
        _predicate = predicate;
    }

    public IQueryable<T> Apply(IQueryable<T> source) => source.Where(_predicate);
}

// Keeps only board cards with the given progress status.
public class CardStatusFilterStep : FilterStep<BoardCard>
{
    public CardStatusFilterStep(CardStatus status)
        : base(c => c.Status == status)
    {
    }
}

// Keeps only board cards of the given category.
public class CardTypeFilterStep : FilterStep<BoardCard>
{
    public CardTypeFilterStep(CardType type)
        : base(c => c.Type == type)
    {
    }
}
