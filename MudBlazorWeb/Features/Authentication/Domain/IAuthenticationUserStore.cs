namespace MudBlazorWeb.Features.Authentication.Domain;

public interface IAuthenticationUserStore
{
    Task<AuthenticationUser?> GetByEmailAddressAsync(string? emailAddress, CancellationToken cancellationToken = default);
    Task SaveAsync(AuthenticationUser user, CancellationToken cancellationToken = default);
    Task UpdateAsync(AuthenticationUser user, CancellationToken cancellationToken = default);
}

public class AuthenticationUser
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string? TelephoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string HashedPassword { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public string? EmailVerificationTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public string? PasswordResetTokenExpiry { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    public Guid PackageId { get; set; }
    public int Avatar { get; set; }
    public int UserType { get; set; }
    public string? Skills { get; set; }
    public string? Hobbies { get; set; }

    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}