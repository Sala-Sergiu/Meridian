using System.Linq.Expressions;
using Meridian.Domain.Common;
using Meridian.Domain.Entities;

namespace Meridian.Bll.QueryPipeline.Steps;

// Sorting step (OrderBy / OrderByDescending). Standard LINQ only — no EF Core APIs.
public class SortingStep<T, TKey> : IQueryStep<T>
{
    private readonly Expression<Func<T, TKey>> _keySelector;
    private readonly bool _descending;

    public SortingStep(Expression<Func<T, TKey>> keySelector, bool descending = false)
    {
        _keySelector = keySelector;
        _descending = descending;
    }

    public IQueryable<T> Apply(IQueryable<T> source)
        => _descending ? source.OrderByDescending(_keySelector) : source.OrderBy(_keySelector);
}

// Sorts board cards by their Order column — the board's default ordering.
public class OrderSortStep : SortingStep<BoardCard, int>
{
    public OrderSortStep(bool descending = false)
        : base(c => c.Order, descending)
    {
    }
}
