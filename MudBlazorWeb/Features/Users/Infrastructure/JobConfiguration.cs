using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorWeb.Features.Users.Domain;

namespace MudBlazorWeb.Features.Users.Infrastructure;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs", schema: "users");

        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).ValueGeneratedOnAdd();

        builder.Property(j => j.UserId)
            .IsRequired();

        builder.Property(j => j.Company)
            .HasMaxLength(200);

        builder.Property(j => j.Position)
            .HasMaxLength(200);

        builder.Property(j => j.StartDate)
            .HasMaxLength(50);

        builder.Property(j => j.EndDate)
            .HasMaxLength(50);

        builder.Property(j => j.Responsibilities)
            .HasMaxLength(1000);

        // Foreign key relationship to User
        builder.HasOne<User>()
            .WithMany(u => u.Jobs)
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
