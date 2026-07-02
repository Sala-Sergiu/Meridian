using FluentValidation;
using Mapster;
using Meridian.Bll.Security;
using Meridian.Bll.Services;
using Meridian.Bll.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Bll;

// Composition helpers for the business-logic layer.
public static class DependencyInjection
{
    public static IServiceCollection AddBll(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOnboardingTemplateService, OnboardingTemplateService>();
        services.AddScoped<IOnboardingBoardService, OnboardingBoardService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        // Apply Mapster IRegister mappings from this assembly to the global config
        // used by Adapt<T>().
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);

        return services;
    }
}
