using Mapster;
using Meridian.Bll.Dtos;
using Meridian.Domain.Repositories;

namespace Meridian.Bll.Services;

// Template read logic. Depends on the Domain repository interface only; the
// cached Dal implementation is composed in behind it at the composition root.
public class OnboardingTemplateService : IOnboardingTemplateService
{
    private readonly IOnboardingTemplateRepository _templates;

    public OnboardingTemplateService(IOnboardingTemplateRepository templates)
    {
        _templates = templates;
    }

    public async Task<OnboardingTemplateDto?> GetTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await _templates.GetByIdAsync(id, cancellationToken);
        return template?.Adapt<OnboardingTemplateDto>();
    }

    public async Task<IReadOnlyList<OnboardingTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _templates.GetAllAsync(cancellationToken);
        return templates.Adapt<List<OnboardingTemplateDto>>();
    }
}
