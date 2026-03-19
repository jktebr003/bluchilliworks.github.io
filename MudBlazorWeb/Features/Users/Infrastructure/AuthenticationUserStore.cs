using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Features.Users.Domain;

namespace MudBlazorWeb.Features.Users.Infrastructure;

internal sealed class AuthenticationUserStore : IAuthenticationUserStore
{
    private readonly IUserRepository _userRepository;

    public AuthenticationUserStore(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthenticationUser?> GetByEmailAddressAsync(string? emailAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetUserByEmailAddressAsync(emailAddress, cancellationToken);
        return user == null ? null : ToAuthenticationUser(user);
    }

    public Task SaveAsync(AuthenticationUser user, CancellationToken cancellationToken = default)
    {
        return _userRepository.SaveUserAsync(ToUser(user), cancellationToken);
    }

    public Task UpdateAsync(AuthenticationUser user, CancellationToken cancellationToken = default)
    {
        return _userRepository.UpdateUserAsync(ToUser(user), cancellationToken);
    }

    private static AuthenticationUser ToAuthenticationUser(User user)
    {
        return new AuthenticationUser
        {
            Id = user.Id,
            Name = user.Name,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            EmailAddress = user.EmailAddress,
            TelephoneNumber = user.TelephoneNumber,
            MobileNumber = user.MobileNumber,
            HashedPassword = user.HashedPassword,
            EmailVerified = user.EmailVerified,
            EmailVerificationToken = user.EmailVerificationToken,
            EmailVerificationTokenExpiry = user.EmailVerificationTokenExpiry,
            PasswordResetToken = user.PasswordResetToken,
            PasswordResetTokenExpiry = user.PasswordResetTokenExpiry,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            PackageId = user.PackageId,
            Avatar = user.Avatar,
            UserType = user.UserType,
            Skills = user.Skills,
            Hobbies = user.Hobbies,
            CreatedOn = user.CreatedOn,
            CreatedBy = user.CreatedBy,
            ModifiedOn = user.ModifiedOn,
            ModifiedBy = user.ModifiedBy,
            DeletedOn = user.DeletedOn,
            DeletedBy = user.DeletedBy,
            IsDeleted = user.IsDeleted
        };
    }

    private static User ToUser(AuthenticationUser user)
    {
        return new User
        {
            Id = user.Id,
            Name = user.Name,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            EmailAddress = user.EmailAddress,
            TelephoneNumber = user.TelephoneNumber,
            MobileNumber = user.MobileNumber,
            HashedPassword = user.HashedPassword,
            EmailVerified = user.EmailVerified,
            EmailVerificationToken = user.EmailVerificationToken,
            EmailVerificationTokenExpiry = user.EmailVerificationTokenExpiry,
            PasswordResetToken = user.PasswordResetToken,
            PasswordResetTokenExpiry = user.PasswordResetTokenExpiry,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            PackageId = user.PackageId,
            Avatar = user.Avatar,
            UserType = user.UserType,
            Skills = user.Skills,
            Hobbies = user.Hobbies,
            CreatedOn = user.CreatedOn,
            CreatedBy = user.CreatedBy,
            ModifiedOn = user.ModifiedOn,
            ModifiedBy = user.ModifiedBy,
            DeletedOn = user.DeletedOn,
            DeletedBy = user.DeletedBy,
            IsDeleted = user.IsDeleted
        };
    }
}