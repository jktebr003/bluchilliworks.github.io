namespace MudBlazorWeb.Shared.Models;

public class ChangeUserPasswordRequest
{
    public string NewPassword { get; set; }
    public string? RequestingUserId { get; set; }
}
