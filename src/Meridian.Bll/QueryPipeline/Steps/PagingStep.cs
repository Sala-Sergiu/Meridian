using Meridian.Domain.Common;

namespace Meridian.Bll.QueryPipeline.Steps;

// Paging step (Skip / Take). Standard LINQ only.
public class PagingStep<T> : IQueryStep<T>
{
    public IQueryable<T> Apply(IQueryable<T> source)
    {
        // TODO: apply Skip/Take per spec.
        throw new NotImplementedException();
    }
}
