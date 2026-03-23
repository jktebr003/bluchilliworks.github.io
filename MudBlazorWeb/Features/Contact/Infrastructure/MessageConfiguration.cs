using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MudBlazorWeb.Features.Contact.Domain;

namespace MudBlazorWeb.Features.Contact.Infrastructure;

public class MessageConfiguration: IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages", schema: "system");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.EmailAddress)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(p => p.Subject)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(p => p.Body)
            .HasMaxLength(1000);

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

        var seedCreatedOn = new DateTime(2026, 3, 13, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData([
            new(){ Id = new Guid("e3425ce6-67e6-4318-b522-920f2c96fa41"), Name = "John Doe", EmailAddress = "john.doe@example.com", Subject = "Hello", Body = "This is a test message.", SentOn = seedCreatedOn, Status = 0, AttemptCount = 0, MaxRetries = 3, LastAttemptedOn = null, LastErrorMessage = null, CreatedOn = seedCreatedOn, CreatedBy = "Seeder", ModifiedOn = null, ModifiedBy = null, IsDeleted = false, DeletedOn = null, DeletedBy = null },
            new(){ Id = new Guid("e3425ce6-67e6-4318-b522-920f2c96fa42"), Name = "Jane Smith", EmailAddress = "jane.smith@example.com", Subject = "Hi", Body = "This is another test message.", SentOn = seedCreatedOn, Status = 0, AttemptCount = 0, MaxRetries = 3, LastAttemptedOn = null, LastErrorMessage = null, CreatedOn = seedCreatedOn, CreatedBy = "Seeder", ModifiedOn = null, ModifiedBy = null, IsDeleted = false, DeletedOn = null, DeletedBy = null }
        ]);
    }
}
