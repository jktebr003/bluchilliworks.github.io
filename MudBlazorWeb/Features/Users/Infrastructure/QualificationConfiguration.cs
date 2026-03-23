using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorWeb.Features.Users.Domain;

namespace MudBlazorWeb.Features.Users.Infrastructure;

public class QualificationConfiguration : IEntityTypeConfiguration<Qualification>
{
    public void Configure(EntityTypeBuilder<Qualification> builder)
    {
        builder.ToTable("Qualifications", schema: "users");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).ValueGeneratedOnAdd();

        builder.Property(q => q.UserId)
            .IsRequired();

        builder.Property(q => q.Title)
            .HasMaxLength(200);

        builder.Property(q => q.Institution)
            .HasMaxLength(200);

        builder.Property(q => q.Year);

        // Foreign key relationship to User
        builder.HasOne<User>()
            .WithMany(u => u.Qualifications)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
