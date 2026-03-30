using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorWeb.Features.Authentication.Infrastructure;
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
        builder.Property(p => p.SessionTokenHash)
            .IsRequired()
            .HasMaxLength(128);
        builder.Property(p => p.IdleDuration).IsRequired();
        builder.Property(p => p.LastAccessedOn).IsRequired();
        builder.Property(p => p.ExpiresOn).IsRequired();
        builder.Property(p => p.AbsoluteExpiresOn).IsRequired();
        builder.Property(p => p.RevokedOn);
        builder.Property(p => p.UserStateVersion)
            .IsRequired()
            .HasMaxLength(128);
        builder.Property(p => p.IsExpired).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.HasIndex(p => p.SessionTokenHash).IsUnique();
        builder.HasIndex(p => p.UserId);

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
                SessionTokenHash = SessionTokenHasher.HashToken("seed-session-token-1"),
                IdleDuration = 30,
                LastAccessedOn = seedCreatedOn,
                ExpiresOn = seedCreatedOn.AddMinutes(30),
                AbsoluteExpiresOn = seedCreatedOn.AddHours(8),
                UserStateVersion = "seed-version-1",
                IsActive = true,
                IsExpired = false,
                CreatedOn = seedCreatedOn,
                CreatedBy = "Seeder",
                IsDeleted = false
            },
            new UserSession
            {
                Id = new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e2"),
                UserId = "user456",
                SessionTokenHash = SessionTokenHasher.HashToken("seed-session-token-2"),
                IdleDuration = 45,
                LastAccessedOn = seedCreatedOn,
                ExpiresOn = seedCreatedOn.AddMinutes(45),
                AbsoluteExpiresOn = seedCreatedOn.AddHours(8),
                UserStateVersion = "seed-version-2",
                IsActive = true,
                IsExpired = false,
                CreatedOn = seedCreatedOn,
                CreatedBy = "Seeder",
                IsDeleted = false,
            }
        );
    }
}