using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Dal;

// Composition helpers for the data-access layer.
// Invoked only from the Api composition root (Program.cs).
public static class DependencyInjection
{
    public static IServiceCollection AddDal(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: register MeridianDbContext (SqlServer), repositories,
        // and Scrutor caching decorators per spec.
        return services;
    }
}
