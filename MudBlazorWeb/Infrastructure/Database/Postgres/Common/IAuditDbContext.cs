using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Common;

public interface IAuditDbContext
{
    DbSet<Audit> Audits { get; set; }
    ChangeTracker ChangeTracker { get; }
}
