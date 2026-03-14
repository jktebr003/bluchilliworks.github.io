using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

namespace MudBlazorWeb.Features.Pricing.Domain;

public class Package : BaseAuditableEntity
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public string Price { get; set; }
    public int Type { get; set; }

    // public void Configure(EntityTypeBuilder<Package> builder)
    // {
    //     builder.ToTable("Packages");
    //     builder.HasKey(p => p.Id);
    //     builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
    //     builder.Property(p => p.Description).IsRequired().HasMaxLength(500);
    //     builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
    //     builder.Property(p => p.Price).IsRequired().HasMaxLength(50);
    //     builder.Property(p => p.Type).IsRequired();
        
    //     // Query filter for soft delete
    //     builder.HasQueryFilter(p => !p.IsDeleted);
    // }
}
