using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Meridian.Dal.Persistence;

// Design-time factory used by the EF Core tools (migrations). The connection
// string here is only for design time — it is not used to reach a live database
// when running `dotnet ef migrations add`.
public class MeridianDbContextFactory : IDesignTimeDbContextFactory<MeridianDbContext>
{
    public MeridianDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MeridianDbContext>()
            .UseSqlServer("Server=localhost;Database=Meridian;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new MeridianDbContext(options);
    }
}
