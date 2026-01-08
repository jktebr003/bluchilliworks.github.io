using Api.Infrastructure.Database.MongoDb.Repositories;
using Api.Services;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Extensions;
using Shared.Models;

namespace Api.Features.Users;

/// <summary>
/// Handles forgot password requests by sending password reset tokens via email
/// </summary>
public static class ForgotPassword
{
    /// <summary>
    /// Command to initiate password reset
    /// </summary>
    public class Command : IRequest<ApiResult<string>>
    {
        /// <summary>
        /// Email address of the user requesting password reset
        /// </summary>
        public string? EmailAddress { get; set; }
    }

    /// <summary>
    /// Validator for forgot password command
    /// </summary>
    public class Validator : AbstractValidator<Command>
    {
        /// <summary>
        /// Initializes validator rules
        /// </summary>
        public Validator()
        {
            RuleFor(c => c.EmailAddress)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Invalid email address format");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ApiResult<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IValidator<Command> _validator;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public Handler(
            IUserRepository userRepository, 
            IValidator<Command> validator,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _validator = validator;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<ApiResult<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ApiResult<string>(string.Empty, false, "ForgotPassword.Validation", errors);
            }

            // Find user by email
            var user = await _userRepository.GetUserByEmailAddressAsync(request.EmailAddress);
            
            // For security: Always return success even if user doesn't exist
            // This prevents email enumeration attacks
            if (user == null)
            {
                return new ApiResult<string>(string.Empty, true, "ForgotPassword.Success", 
                    "If an account with that email exists, you will receive a password reset link.");
            }

            // Generate secure password reset token (6-digit code for simplicity, or could use GUID)
            var resetToken = SecurityExtension.CreateRandomVerificationCode(6);
            var tokenExpiry = DateTime.UtcNow.AddHours(1); // 1 hour expiry for password reset

            // Store token in user record
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = tokenExpiry.ToString("o");
            user.ModifiedOn = DateTime.UtcNow.ToString("o");
            user.ModifiedBy = "system";

            await _userRepository.SaveUserAsync(user);

            // Send password reset email
            var appBaseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5000";
            var resetLink = $"{appBaseUrl}/authentication/reset-password?email={Uri.EscapeDataString(request.EmailAddress!)}&token={resetToken}";

            var emailSubject = "Password Reset Request";
            var emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>Password Reset Request</h2>
                        <p>Hello {user.FirstName},</p>
                        <p>We received a request to reset your password. If you didn't make this request, you can safely ignore this email.</p>
                        
                        <p>Your password reset code is:</p>
                        <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; text-align: center; margin: 20px 0;'>
                            <strong style='font-size: 24px; letter-spacing: 5px; color: #2c3e50;'>{resetToken}</strong>
                        </div>
                        
                        <p>Or click the link below to reset your password:</p>
                        <div style='text-align: center; margin: 20px 0;'>
                            <a href='{resetLink}' style='background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a>
                        </div>
                        
                        <p style='color: #e74c3c; font-weight: bold;'>This code will expire in 1 hour.</p>
                        
                        <p style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 12px;'>
                            If you're having trouble clicking the button, copy and paste the URL below into your web browser:<br/>
                            <a href='{resetLink}' style='color: #3498db;'>{resetLink}</a>
                        </p>
                        
                        <p style='color: #666; font-size: 12px;'>
                            This is an automated message, please do not reply to this email.
                        </p>
                    </div>
                </body>
                </html>";

            try
            {
                await _emailService.SendEmailAsync(
                    to: request.EmailAddress!,
                    subject: emailSubject,
                    body: emailBody,
                    fromName: "BluChilli Works",
                    fromEmail: null,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                // Log error but don't reveal it to user
                // In production, you'd log this properly
                Console.WriteLine($"Failed to send password reset email: {ex.Message}");
            }

            return new ApiResult<string>(string.Empty, true, "ForgotPassword.Success", 
                "If an account with that email exists, you will receive a password reset link.");
        }
    }
}

/// <summary>
/// Endpoint for forgot password functionality
/// </summary>
public class ForgotPasswordEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the forgot password route
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users/forgot-password", async (ForgotPasswordRequest request, ISender sender) =>
        {
            var result = await sender.Send(request.Adapt<ForgotPassword.Command>());
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AllowAnonymous(); // Allow anonymous access for password reset
    }
}
