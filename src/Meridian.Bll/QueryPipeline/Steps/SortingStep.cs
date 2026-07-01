namespace Meridian.Bll.QueryPipeline.Steps;

// Sorting step (OrderBy / OrderByDescending). Standard LINQ only.
public class SortingStep<T> : IQueryStep<T>
{
    public IQueryable<T> Apply(IQueryable<T> source)
    {
        // TODO: apply ordering per spec.
        throw new NotImplementedException();
    }
}
