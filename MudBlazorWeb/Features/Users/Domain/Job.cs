using System.ComponentModel.DataAnnotations;

namespace MudBlazorWeb.Features.Users.Domain;

public class Job
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Company { get; set; }
    public string? Position { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Responsibilities { get; set; }
}
