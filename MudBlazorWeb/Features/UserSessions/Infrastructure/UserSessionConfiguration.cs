using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorWeb.Features.UserSessions.Domain;

namespace MudBlazorWeb.Features.UserSessions.Infrastructure;

public class UserSessionConfiguration: IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions", schema: "users");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(p => p.SessionToken)
            .IsRequired()
            .HasMaxLength(200);        
        builder.Property(p => p.IdleDuration).IsRequired();
        builder.Property(p => p.LastAccessedOn);
        builder.Property(p => p.ExpiresOn);
        builder.Property(p => p.IsExpired);
        builder.Property(p => p.IsActive);

        // Audit fields from BaseAuditableEntity
        builder.Property(p => p.CreatedOn).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.ModifiedOn);
        builder.Property(p => p.ModifiedBy).HasMaxLength(100);
        builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.DeletedOn);
        builder.Property(p => p.DeletedBy).HasMaxLength(100);

        // Global query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);

        var seedCreatedOn = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new UserSession
            {
                Id = new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e1"),
                UserId = "user123",
                SessionToken = "sessiontoken123",
                IdleDuration = 30,
                ExpiresOn = seedCreatedOn.AddHours(1).ToString("o"),
                IsActive = true,
                CreatedOn = seedCreatedOn,
                CreatedBy = "Seeder",
                IsDeleted = false
            },
            new UserSession
            {
                Id = new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e2"),
                UserId = "user456",
                SessionToken = "sessiontoken456",
                IdleDuration = 45,
                ExpiresOn = seedCreatedOn.AddHours(2).ToString("o"),
                IsActive = true,
                CreatedOn = seedCreatedOn,
                CreatedBy = "Seeder",
                IsDeleted = false,
            }
        );
    }
}