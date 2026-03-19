using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;

using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Authentication.Application;

internal static class AuthenticationCommandHelpers
{
    public static UserResponse MapToUserResponse(AuthenticationUser user)
    {
        return new UserResponse
        {
            ID = user.Id.ToString(),
            Name = user.Name,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            EmailAddress = user.EmailAddress,
            TelephoneNumber = user.TelephoneNumber,
            MobileNumber = user.MobileNumber,
            EmailVerified = user.EmailVerified,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            UserRole = (UserType)user.UserType,
            Skills = user.Skills,
            Hobbies = user.Hobbies,
            CreatedOn = user.CreatedOn.ToString("O", CultureInfo.InvariantCulture),
            CreatedBy = user.CreatedBy,
            ModifiedOn = user.ModifiedOn?.ToString("O", CultureInfo.InvariantCulture),
            ModifiedBy = user.ModifiedBy,
            DeletedOn = user.DeletedOn?.ToString("O", CultureInfo.InvariantCulture),
            DeletedBy = user.DeletedBy,
            IsDeleted = user.IsDeleted
        };
    }

    public static UserResponse EmptyUserResponse()
    {
        return new UserResponse
        {
            ID = string.Empty,
            CreatedOn = string.Empty,
            CreatedBy = string.Empty
        };
    }

    public static string? ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password is required.";
        }

        if (password.Length < 8)
        {
            return "Password must be at least 8 characters long.";
        }

        if (!password.Any(char.IsUpper))
        {
            return "Password must contain at least one uppercase letter.";
        }

        if (!password.Any(char.IsLower))
        {
            return "Password must contain at least one lowercase letter.";
        }

        if (!password.Any(char.IsDigit))
        {
            return "Password must contain at least one number.";
        }

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return "Password must contain at least one special character.";
        }

        return null;
    }

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool VerifyPassword(string hashedPassword, string password)
    {
        var computedHash = HashPassword(password);
        return string.Equals(hashedPassword, computedHash, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsExpired(string? expiryValue)
    {
        if (string.IsNullOrWhiteSpace(expiryValue))
        {
            return false;
        }

        if (!DateTime.TryParse(
                expiryValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var expiry))
        {
            return false;
        }

        return expiry < DateTime.UtcNow;
    }

    public static string GetBaseWebUrl(IConfiguration configuration)
    {
        return configuration["BaseWebUrl"]
            ?? configuration["App:BaseUrl"]
            ?? "http://localhost:5001";
    }

    public static string BuildVerificationEmailBody(string firstName, string verificationToken, string verificationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .code {{ font-size: 24px; font-weight: bold; color: #4CAF50; letter-spacing: 2px; padding: 15px; background-color: #e8f5e9; border-radius: 4px; text-align: center; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Welcome to BluChilliWorks</h1>
        </div>
        <div class=""content"">
            <p>Hello {firstName},</p>
            <p>Use the verification code below to verify your email and set up your password:</p>
            <div class=""code"">{verificationToken}</div>
            <p style=""text-align: center;""><a href=""{verificationLink}"" class=""button"">Set Up Your Password</a></p>
            <p><strong>This verification code will expire in 24 hours.</strong></p>
        </div>
    </div>
</body>
</html>";
    }

    public static string BuildResetPasswordEmailBody(string firstName, string resetToken, string resetLink)
    {
        return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #2c3e50;'>Password Reset Request</h2>
        <p>Hello {firstName},</p>
        <p>Your password reset code is:</p>
        <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; text-align: center; margin: 20px 0;'>
            <strong style='font-size: 24px; letter-spacing: 5px; color: #2c3e50;'>{resetToken}</strong>
        </div>
        <p style='text-align: center; margin: 20px 0;'>
            <a href='{resetLink}' style='background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a>
        </p>
        <p style='color: #e74c3c; font-weight: bold;'>This code will expire in 1 hour.</p>
    </div>
</body>
</html>";
    }

    public static async Task<bool> TrySendEmailAsync(
        IConfiguration configuration,
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        try
        {
            var smtpHost = configuration["Email:Smtp:Host"];
            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                return false;
            }

            var smtpPort = int.TryParse(configuration["Email:Smtp:Port"], out var configuredPort) ? configuredPort : 587;
            var smtpUsername = configuration["Email:Smtp:Username"];
            var smtpPassword = configuration["Email:Smtp:Password"];
            var enableSsl = !bool.TryParse(configuration["Email:Smtp:EnableSsl"], out var configuredSsl) || configuredSsl;
            var fromEmail = configuration["Email:FromEmail"] ?? "noreply@bluchilliworks.com";
            var fromName = configuration["Email:FromName"] ?? "BluChilli Works";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                smtpHost,
                smtpPort,
                enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(smtpUsername) && !string.IsNullOrWhiteSpace(smtpPassword))
            {
                await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}