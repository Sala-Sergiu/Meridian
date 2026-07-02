namespace Meridian.Bll.Dtos;

// Paged projection exposed across the API boundary. TotalCount reflects the
// filtered set before paging, so clients can build pagers.
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
