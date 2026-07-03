using Meridian.Bll.Dtos;

namespace Meridian.Bll.Services;

// HR-owned onboarding templates: reads for everyone, publishing for HR.
public interface IOnboardingTemplateService
{
    // Null when no template with the given id exists.
    Task<OnboardingTemplateDto?> GetTemplateAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OnboardingTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default);

    // Publishes a new article: appends it to the template (future hires get
    // it via cloning) and pushes a ToDo copy onto every EXISTING board — the
    // one deliberate exception to "boards never change after cloning", as an
    // explicit HR broadcast. Null when the template does not exist.
    Task<PublishCardResultDto?> PublishCardAsync(int templateId, PublishCardRequestDto request, CancellationToken cancellationToken = default);
}
