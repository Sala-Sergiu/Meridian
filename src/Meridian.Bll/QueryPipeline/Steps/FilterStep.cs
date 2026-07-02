using Meridian.Domain.Common;

namespace Meridian.Bll.QueryPipeline.Steps;

// Filtering step (Where). Standard LINQ only.
public class FilterStep<T> : IQueryStep<T>
{
    public IQueryable<T> Apply(IQueryable<T> source)
    {
        // TODO: apply Where predicate per spec.
        throw new NotImplementedException();
    }
}
