namespace Shared.Models;

public class ResetPasswordRequest
{
    public string? EmailAddress { get; set; }
    public string? ResetToken { get; set; }
    public string? NewPassword { get; set; }
}
