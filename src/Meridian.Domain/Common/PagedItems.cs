namespace Meridian.Domain.Common;

// Materialized page of a query: the windowed items plus the total count of
// the filtered-but-unpaged set. What repositories return instead of ever
// letting IQueryable escape Dal.
public record PagedItems<T>(IReadOnlyList<T> Items, int TotalCount);
