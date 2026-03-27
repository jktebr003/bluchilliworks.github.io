using System.ComponentModel.DataAnnotations;
using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

namespace MudBlazorWeb.Features.Users.Domain;

public class Qualification : BaseAuditableEntity
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public string? Institution { get; set; }
    public int? Year { get; set; }
}
