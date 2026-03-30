using System.Security.Cryptography;
using System.Text;

namespace MudBlazorWeb.Features.Authentication.Infrastructure;

internal static class SessionTokenHasher
{
    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}