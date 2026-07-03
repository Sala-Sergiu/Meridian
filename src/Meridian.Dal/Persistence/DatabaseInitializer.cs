using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Meridian.Dal.Persistence;

// Applies pending EF migrations (schema + HasData seed) at startup. Meant for
// containerized runs, where there is no operator to run `dotnet ef database
// update` — enable via Database:ApplyMigrationsAtStartup. Local dev keeps
// using the CLI so a plain F5 never mutates the database.
//
// Invoked only from the Api composition root (Program.cs).
public static class DatabaseInitializer
{
    // SQL Server in a fresh container can accept TCP connections a moment
    // before it is actually ready to create databases, so a short retry loop
    // backs up the compose healthcheck.
    public static async Task ApplyMigrationsAsync(
        IServiceProvider services,
        int maxAttempts = 10,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MeridianDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MeridianDbContext>>();

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Database migrations applied (attempt {Attempt}).", attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Database not ready (attempt {Attempt}/{MaxAttempts}); retrying in 3s.",
                    attempt,
                    maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }
    }
}
