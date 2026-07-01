using Meridian.Domain.Common;
using Meridian.Domain.Entities;

namespace Meridian.Domain.Repositories;

// Repository contract for onboarding templates. Implemented in Dal, where
// GetByIdAsync/GetAllAsync load templates together with their cards.
// This interface is also the caching seam: reads are decorated with a cache
// in Dal because templates are read on every board view and change rarely.
public interface IOnboardingTemplateRepository : IRepository<OnboardingTemplate>
{
}
