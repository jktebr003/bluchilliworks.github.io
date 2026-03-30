using System.Security.Cryptography;
using System.Text;

using MudBlazorWeb.Features.Authentication.Domain;

namespace MudBlazorWeb.Features.Authentication.Infrastructure;

internal static class SessionUserStateVersionFactory
{
    public static string Create(AuthenticationUser user)
    {
        var source = string.Join(
            '|',
            user.Id,
            user.HashedPassword,
            user.EmailVerified,
            user.IsDeleted,
            user.UserType);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes);
    }
}