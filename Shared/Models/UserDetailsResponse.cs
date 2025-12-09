using Shared.Enums;

namespace Shared.Models;

/// <summary>
/// Detailed user information accessible only by staff members
/// </summary>
public class UserDetailsResponse : BaseResponse
{
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? EmailAddress { get; set; }
    public bool EmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public string? EmailVerificationTokenExpiry { get; set; }
    public bool IsDeleted { get; set; }
    public string? HashedPassword { get; set; }
    public UserType UserRole { get; set; }
}
