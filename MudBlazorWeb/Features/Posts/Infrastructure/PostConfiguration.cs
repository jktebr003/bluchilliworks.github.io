using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorWeb.Features.Posts.Domain;

namespace MudBlazorWeb.Features.Posts.Infrastructure;

public class PostConfiguration: IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts", schema: "content");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.Heading)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(p => p.Author)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.Content)
            .HasMaxLength(1000);
        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(p => p.Likes)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]");

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

        var seedCreatedOn = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData([
            new { Id = new Guid("6a202a02-654c-4e2e-9730-a30174d5eb41"), Title = "First Post", Description = "This is the first post.", Author = "John Doe", Heading = "First Post Heading", Content = "This is a test message.", UserId = "seed-user-1", Category = "General", TotalViews = 0, PostedOn = seedCreatedOn, CreatedOn = seedCreatedOn, CreatedBy = "Seeder", ModifiedOn = (DateTime?)null, ModifiedBy = (string?)null, IsDeleted = false, DeletedOn = (DateTime?)null, DeletedBy = (string?)null },
            new { Id = new Guid("6a202a02-654c-4e2e-9730-a30174d5eb42"), Title = "Second Post", Description = "This is the second post.", Author = "Jane Smith", Heading = "Second Post Heading", Content = "This is another test message.", UserId = "seed-user-2", Category = "General", TotalViews = 0, PostedOn = seedCreatedOn, CreatedOn = seedCreatedOn, CreatedBy = "Seeder", ModifiedOn = (DateTime?)null, ModifiedBy = (string?)null, IsDeleted = false, DeletedOn = (DateTime?)null, DeletedBy = (string?)null }
        ]);
    }
}
