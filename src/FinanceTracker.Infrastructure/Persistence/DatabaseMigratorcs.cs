using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    /// <summary>Lefuttatja a függőben lévő migrációkat; az adatbázis indulására várva újrapróbálkozik.</summary>
    public static async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigrator");
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!db.Database.GetMigrations().Any())
        {
            logger.LogWarning(
                "Nincs EF migráció, az adatbázis séma nem jön létre. Hozd létre az első migrációt: " +
                "dotnet ef migrations add InitialCreate -p FinanceTracker.Infrastructure -s FinanceTracker.API -o Persistence/Migrations");
            return;
        }

        const int maxAttempts = 10;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Az adatbázis naprakész.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "A migráció sikertelen ({Attempt}/{Max}), újrapróbálkozás...", attempt, maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }
    }
}
