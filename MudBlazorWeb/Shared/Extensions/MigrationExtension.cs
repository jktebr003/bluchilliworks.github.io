using Microsoft.EntityFrameworkCore;

using MudBlazorWeb.Infrastructure.Database.Postgres;

namespace MudBlazorWeb.Shared.Extensions;

public static class MigrationExtension
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        appDbContext.Database.Migrate();
    }
}
