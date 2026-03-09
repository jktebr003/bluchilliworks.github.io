using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Common;

public class Audit : BaseEntity, IEntityTypeConfiguration<Audit>
{
    public Audit()
    {
        Id = Guid.NewGuid();
    }
    public Guid Id { get; set; }
    public string? AuditType { get; set; }           /*Create, Update or Delete*/
    public string? AuditUser { get; set; }           /*Log User*/
    public string? TableName { get; set; }           /*Table where rows been created/updated/deleted*/
    public string? KeyValues { get; set; }           /*Table Pk and it's values*/
    public string? OldValues { get; set; }           /*Changed column name and old value*/
    public string? NewValues { get; set; }           /*Changed column name  and current value*/
    public string? ChangedColumns { get; set; }      /*Changed column names*/


    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
    }
}
