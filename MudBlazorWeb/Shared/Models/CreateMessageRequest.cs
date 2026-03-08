namespace MudBlazorWeb.Shared.Models;

public class CreateMessageRequest
{
    public string? From { get; set; }

    public string? EmailAddress { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public string? SentOn { get; set; }

    public string CreatedBy { get; set; } = "system";
}
