using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorWeb.Features.Users.Domain;

namespace MudBlazorWeb.Features.Users.Infrastructure;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.Username)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(p => p.EmailAddress)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(p => p.TelephoneNumber)
            .HasMaxLength(50);
        builder.Property(p => p.MobileNumber)
            .HasMaxLength(50);
        builder.Property(p => p.HashedPassword)
            .IsRequired()
            .HasMaxLength(100);

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

        // builder.HasData([
        //     new { Id = new Guid("6a202a02-654c-4e2e-9730-a30174d5eb41"), Title = "First Post", Description = "This is the first post.", Author = "John Doe", Heading = "First Post Heading", Content = "This is a test message.", UserId = "seed-user-1", Category = "General", TotalViews = 0, PostedOn = seedCreatedOn, CreatedOn = seedCreatedOn, CreatedBy = "Seeder", ModifiedOn = (DateTime?)null, ModifiedBy = (string?)null, IsDeleted = false, DeletedOn = (DateTime?)null, DeletedBy = (string?)null },
        //     new { Id = new Guid("6a202a02-654c-4e2e-9730-a30174d5eb42"), Title = "Second Post", Description = "This is the second post.", Author = "Jane Smith", Heading = "Second Post Heading", Content = "This is another test message.", UserId = "seed-user-2", Category = "General", TotalViews = 0, PostedOn = seedCreatedOn, CreatedOn = seedCreatedOn, CreatedBy = "Seeder", ModifiedOn = (DateTime?)null, ModifiedBy = (string?)null, IsDeleted = false, DeletedOn = (DateTime?)null, DeletedBy = (string?)null }
        // ]);
        builder.HasData(
            new User
            {
                Id = new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e1"),
                Name = "John Doe",
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                EmailAddress = "johndoe@example.com",
                HashedPassword = "hashedpassword123",
                CreatedOn = seedCreatedOn,
                CreatedBy = "Seeder",
                IsDeleted = false
            },
            new User
            {
                Id = new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e2"),
                Name = "Jane Smith",
                FirstName = "Jane",
                LastName = "Smith",
                Username = "janesmith",
                EmailAddress = "janesmith@example.com",
                HashedPassword = "hashedpassword123",
                CreatedOn = seedCreatedOn,
                CreatedBy = "Seeder",
                IsDeleted = false
            }

        );

    }
}
