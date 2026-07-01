using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Bll;

// Composition helpers for the business-logic layer.
public static class DependencyInjection
{
    public static IServiceCollection AddBll(this IServiceCollection services)
    {
        // TODO: register services, FluentValidation validators,
        // Mapster config and query-pipeline steps per spec.
        return services;
    }
}
