using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Common;

public class AuditHelper
{
    readonly IAuditDbContext Db;

    public AuditHelper(IAuditDbContext db)
    {
        Db = db;
    }

    public void AddAuditLogs(string userName)
    {
        Db.ChangeTracker.DetectChanges();
        List<AuditEntry> auditEntries = new();
        foreach (EntityEntry entry in Db.ChangeTracker.Entries())
        {
            if (entry.Entity is Audit || entry.State == EntityState.Detached ||
                entry.State == EntityState.Unchanged)
            {
                continue;
            }
            var auditEntry = new AuditEntry(entry, userName);
            auditEntries.Add(auditEntry);
        }

        if (auditEntries.Any())
        {
            var logs = auditEntries.Select(x => x.ToAudit());
            Db.Audits.AddRange(logs);
        }
    }
}
