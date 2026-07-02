namespace Meridian.Domain.Common;

// A composable query step. Operates on IQueryable<T> using ONLY standard
// System.Linq operators (Where/OrderBy/Skip/Take) — never EF Core APIs.
// The contract sits in Domain (pure BCL, Domain still depends on nothing) so
// repository interfaces can accept steps and Dal — which references Domain
// only — can apply them. Concrete steps and their composition live in Bll.
public interface IQueryStep<T>
{
    IQueryable<T> Apply(IQueryable<T> source);
}
