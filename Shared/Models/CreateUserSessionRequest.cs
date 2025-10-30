using Shared.Extensions;

namespace Shared.Models;

public class CreateUserSessionRequest
{
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public DateTime CreatedOn { get; set; } = DateTimeExtension.GetSouthAfricanTime();
    public string CreatedBy { get; set; } = "system";
}
