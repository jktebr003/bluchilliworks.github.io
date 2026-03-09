namespace MudBlazorWeb.Infrastructure.Database.Postgres.Common;

public class BaseEntity
{
    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date when the entity was created.
    /// </summary>
    public DateTime? CreatedDate { get; set; }
}
