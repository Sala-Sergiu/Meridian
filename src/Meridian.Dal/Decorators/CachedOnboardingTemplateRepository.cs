using Meridian.Domain.Entities;
using Meridian.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Meridian.Dal.Decorators;

// Caching decorator over IOnboardingTemplateRepository, registered via Scrutor.
//
// This is the designated hot path for caching: the HR onboarding template is
// read on every board view by every user, but HR edits it rarely — a short
// in-memory TTL removes the repeated template+cards query without risking
// long-lived staleness. Per CLAUDE.md, caching applies ONLY here; other
// repositories stay undecorated.
public class CachedOnboardingTemplateRepository : IOnboardingTemplateRepository
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);

    private const string AllKey = "onboarding-template:all";
    private const string ByIdKeyPrefix = "onboarding-template:";

    private readonly IOnboardingTemplateRepository _inner;
    private readonly IMemoryCache _cache;

    public CachedOnboardingTemplateRepository(IOnboardingTemplateRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<OnboardingTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var key = $"{ByIdKeyPrefix}{id}";
        if (_cache.TryGetValue(key, out OnboardingTemplate? cached))
        {
            return cached;
        }

        var template = await _inner.GetByIdAsync(id, cancellationToken);
        if (template is not null)
        {
            // Misses are not cached: an unknown id should stay a cheap lookup
            // and become visible immediately once the template exists.
            _cache.Set(key, template, Ttl);
        }

        return template;
    }

    public async Task<IReadOnlyList<OnboardingTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(AllKey, out IReadOnlyList<OnboardingTemplate>? cached) && cached is not null)
        {
            return cached;
        }

        var templates = await _inner.GetAllAsync(cancellationToken);
        _cache.Set(AllKey, templates, Ttl);

        return templates;
    }
}
