namespace Meridian.Domain.Common;

// Marker for steps that window the result set (Skip/Take). The repository
// applies every non-paging step first, computes TotalCount there — so the
// total reflects the filtered-but-unpaged set — and only then applies these.
public interface IPagingStep<T> : IQueryStep<T>
{
}
