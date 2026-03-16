using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MudBlazorWeb.Features.Contact.Domain;
using MudBlazorWeb.Features.Contact.Infrastructure;
using MudBlazorWeb.Features.Posts.Domain;
using MudBlazorWeb.Features.Posts.Infrastructure;
using MudBlazorWeb.Features.Pricing.Domain;
using MudBlazorWeb.Features.Pricing.Infrastructure;
using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Features.Users.Infrastructure;
using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

namespace MudBlazorWeb.Infrastructure.Database.Postgres;

public class AppDbContext : DbContext, IAuditDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets for each feature
    public DbSet<Audit> Audits { get; set; }
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<User> Users => Set<User>();

    public string GenerateReferenceNumber<T>() where T : class
    {
        if (_acronyms.TryGetValue(typeof(T), out var acronym))
        {
            var today = DateTime.UtcNow.Date;
            var southAfricaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
            var todayStart = TimeZoneInfo.ConvertTimeFromUtc(today, southAfricaTimeZone);
            var todayEnd = todayStart.AddDays(1);

            var count = Set<T>().Count(e => EF.Property<DateTime>(e, "CreatedDate") >= todayStart && EF.Property<DateTime>(e, "CreatedDate") < todayEnd);

            return $"{DateTime.Now.Year}/" +
                   $"{DateTime.Now.Month}/" +
                   $"{DateTime.Now.Day}/" +
                   $"{acronym}/" +
                   $"{(count + 1).ToString().PadLeft(3, '0')}/" +
                   $"{acronym}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }

        throw new InvalidOperationException($"No acronym defined for type {typeof(T).Name}");
    }

    public int SaveChanges(string userName, CancellationToken cancellationToken = default)
    {
        new AuditHelper(this).AddAuditLogs(userName);
        var result = SaveChanges();
        return result;
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries<BaseAuditableEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = DateTime.UtcNow;
            }

            NormalizeDateTimesToUtc(entry);
        }

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Handle audit fields
        var entries = ChangeTracker.Entries<BaseAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOn = DateTime.UtcNow;
                    entry.Entity.CreatedBy = "System"; // Get from auth context
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedOn = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = "System"; // Get from auth context
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedOn = DateTime.UtcNow;
                    entry.Entity.DeletedBy = "System"; // Get from auth context
                    entry.Entity.IsDeleted = true;
                    break;
            }

            NormalizeDateTimesToUtc(entry);
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void NormalizeDateTimesToUtc(EntityEntry<BaseAuditableEntity> entry)
    {
        foreach (var property in entry.Properties)
        {
            if (property.Metadata.ClrType == typeof(DateTime))
            {
                var value = property.CurrentValue as DateTime?;
                if (value.HasValue)
                {
                    property.CurrentValue = NormalizeToUtc(value.Value);
                }
            }
            else if (property.Metadata.ClrType == typeof(DateTime?))
            {
                var value = property.CurrentValue as DateTime?;
                if (value.HasValue)
                {
                    property.CurrentValue = NormalizeToUtc(value.Value);
                }
            }
        }
    }

    private static DateTime NormalizeToUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Explicitly apply configurations to avoid reflection issues
        modelBuilder.ApplyConfiguration(new AuditConfiguration());
        modelBuilder.ApplyConfiguration(new PackageConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        modelBuilder.ApplyConfiguration(new PostConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());

        // Alternative: If you want to keep assembly scanning, be more specific
        // modelBuilder.ApplyConfigurationsFromAssembly(
        //     typeof(AppDbContext).Assembly,
        //     t => t.Namespace?.StartsWith("MudBlazorWeb.Infrastructure") == true ||
        //          t.Namespace?.StartsWith("MudBlazorWeb.Features") == true
        // );
    }

    private readonly Dictionary<Type, string> _acronyms = new()
    {
        { typeof(Package), "PKG" },
        { typeof(Message), "MSG" },
        { typeof(Post), "PST" },
        { typeof(User), "USR" },
    };
}
