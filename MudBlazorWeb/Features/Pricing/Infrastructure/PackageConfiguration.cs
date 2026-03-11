using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorWeb.Features.Pricing.Domain;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Pricing.Infrastructure;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ToTable("Packages");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Type)
            .IsRequired();

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

        var seedCreatedOn = new DateTime(2026, 3, 11, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData([
            new(){ Id = new Guid("10905436-1671-4900-9d0e-6fced3fbd3d1"), Name = "Individual Educator", Description = "1 educator", Code = "INDV-1E-MTH", Price = "R75.00", Type = (int)PackageType.Monthly, CreatedOn = seedCreatedOn, CreatedBy = "System"},
            new(){ Id = new Guid("10905436-1671-4900-9d0e-6fced3fbd3d2"), Name = "Individual Educator", Description = "1 educator", Code = "INDV-1E-2YR", Price = "R45.00", Type = (int)PackageType.TwoYearSubscription, CreatedOn = seedCreatedOn, CreatedBy = "System"},
            new(){ Id = new Guid("10905436-1671-4900-9d0e-6fced3fbd3d3"), Name = "School Sign On", Description = "Up to 20 educators", Code = "SCH-20E-MTH", Price = "R650.00", Type = (int)PackageType.Monthly, CreatedOn = seedCreatedOn, CreatedBy = "System"},
            new(){ Id = new Guid("10905436-1671-4900-9d0e-6fced3fbd3d4"), Name = "School Sign On", Description = "Up to 20 educators", Code = "SCH-20E-2YR", Price = "R425.00", Type = (int)PackageType.TwoYearSubscription, CreatedOn = seedCreatedOn, CreatedBy = "System"},
            new(){ Id = new Guid("10905436-1671-4900-9d0e-6fced3fbd3d5"), Name = "School Sign On", Description = "Up to 40 educators", Code = "SCH-40E-MTH", Price = "R900.00", Type = (int)PackageType.Monthly, CreatedOn = seedCreatedOn, CreatedBy = "System"},
            new(){ Id = new Guid("10905436-1671-4900-9d0e-6fced3fbd3d6"), Name = "School Sign On", Description = "Up to 40 educators", Code = "SCH-40E-2YR", Price = "R650.00", Type = (int)PackageType.TwoYearSubscription, CreatedOn = seedCreatedOn, CreatedBy = "System"},
            ]);
    }
}
