namespace MudBlazorWeb.Shared.Models;

public class SetPasswordRequest
{
    public string EmailAddress { get; set; } = string.Empty;
    public string VerificationToken { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
