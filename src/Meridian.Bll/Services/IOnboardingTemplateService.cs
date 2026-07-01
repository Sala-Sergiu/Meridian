using Meridian.Bll.Dtos;

namespace Meridian.Bll.Services;

// Read access to HR-owned onboarding templates. Returns DTOs only.
public interface IOnboardingTemplateService
{
    // Null when no template with the given id exists.
    Task<OnboardingTemplateDto?> GetTemplateAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OnboardingTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default);
}
