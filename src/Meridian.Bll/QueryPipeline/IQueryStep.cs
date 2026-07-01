namespace Meridian.Bll.QueryPipeline;

// A composable query step. Operates on IQueryable<T> using ONLY standard
// System.Linq operators (Where/OrderBy/Skip/Take) — no EF Core APIs here,
// so the BLL stays free of any EF reference.
public interface IQueryStep<T>
{
    IQueryable<T> Apply(IQueryable<T> source);
}
