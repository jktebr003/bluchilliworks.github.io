using System.ComponentModel.DataAnnotations;

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Entities;

public abstract class BaseAuditableEntity
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}
