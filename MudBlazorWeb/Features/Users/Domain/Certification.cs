using System.ComponentModel.DataAnnotations;

namespace MudBlazorWeb.Features.Users.Domain;

public class Certification
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public string? Institution { get; set; }
    public int? Year { get; set; }
}
