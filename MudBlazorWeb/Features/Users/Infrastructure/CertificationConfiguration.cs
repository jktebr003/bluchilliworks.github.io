using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorWeb.Features.Users.Domain;

namespace MudBlazorWeb.Features.Users.Infrastructure;

public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.ToTable("Certifications", schema: "users");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.Title)
            .HasMaxLength(200);

        builder.Property(c => c.Institution)
            .HasMaxLength(200);

        builder.Property(c => c.Year);

        // Foreign key relationship to User
        builder.HasOne<User>()
            .WithMany(u => u.Certifications)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
