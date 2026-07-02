using Meridian.Domain.Common;

namespace Meridian.Bll.QueryPipeline.Steps;

// Paging step (Skip / Take). Standard LINQ only — no EF Core APIs. Implements
// the IPagingStep marker so the repository counts the set before applying it.
public class PagingStep<T> : IPagingStep<T>
{
    private readonly int _page;
    private readonly int _pageSize;

    public PagingStep(int page, int pageSize)
    {
        _page = page;
        _pageSize = pageSize;
    }

    public IQueryable<T> Apply(IQueryable<T> source)
        => source.Skip((_page - 1) * _pageSize).Take(_pageSize);
}
