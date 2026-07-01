using Meridian.Dal.Persistence;
using Meridian.Dal.Repositories;
using Meridian.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Dal;

// Composition helpers for the data-access layer.
// Invoked only from the Api composition root (Program.cs).
public static class DependencyInjection
{
    public static IServiceCollection AddDal(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MeridianDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsAssembly(typeof(MeridianDbContext).Assembly.FullName)));

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
