using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Common;

public class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.ToTable("Audits", schema: "audit");

        builder.HasKey(a => a.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.AuditUser)
            .HasMaxLength(100);

        builder.Property(a => a.AuditType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.TableName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.CreatedDate)
            .IsRequired();

        // JSON columns for PostgreSQL
        builder.Property(a => a.OldValues)
            .HasColumnType("jsonb");

        builder.Property(a => a.NewValues)
            .HasColumnType("jsonb");

        builder.Property(a => a.ChangedColumns)
            .HasColumnType("jsonb");
    }
}
