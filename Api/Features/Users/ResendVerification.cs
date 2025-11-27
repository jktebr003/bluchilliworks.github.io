using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Repositories;
using Api.Services;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Extensions;
using Shared.Models;

namespace Api.Features.Users;

public static class ResendVerification
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string? EmailAddress { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
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
                return new ApiResult<string>(string.Empty, false, "ResendVerification.Validation", errors);
            }

            // Find user by email
            var user = await _userRepository.GetUserByEmailAddressAsync(request.EmailAddress);
            if (user == null)
            {
                // Don't reveal if user exists or not for security
                return new ApiResult<string>(string.Empty, true, "ResendVerification.Success", "If an account exists with this email, a verification email has been sent.");
            }

            // Check if already verified
            if (user.EmailVerified)
            {
                return new ApiResult<string>(string.Empty, false, "ResendVerification.AlreadyVerified", "This email address has already been verified.");
            }

            // Generate new verification token with 24-hour expiry
            var verificationToken = SecurityExtension.CreateRandomVerificationCode(6);
            var tokenExpiry = DateTime.UtcNow.AddHours(24);

            // Update user with new token
            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpiry = tokenExpiry.ToString("o");
            user.ModifiedOn = DateTime.UtcNow.ToString("o");
            user.ModifiedBy = "system";

            await _userRepository.SaveUserAsync(user);

            // Send verification email
            var baseUrl = _configuration.GetValue<string>("App:BaseUrl") ?? "http://localhost:5000";
            var verificationLink = $"{baseUrl}/authentication/setup-password?token={verificationToken}&email={Uri.EscapeDataString(request.EmailAddress)}";

            var emailBody = $@"
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
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Email Verification</h1>
        </div>
        <div class=""content"">
            <p>Hello {user.FirstName},</p>
            <p>You requested a new verification code for your BluChilliWorks account. Use the code below to verify your email and set up your password:</p>
            <div class=""code"">{verificationToken}</div>
            <p>Or click the button below to set up your password:</p>
            <p style=""text-align: center;"">
                <a href=""{verificationLink}"" class=""button"">Set Up Your Password</a>
            </p>
            <p><strong>This verification code will expire in 24 hours.</strong></p>
            <p>If you didn't request this code, please ignore this email.</p>
        </div>
        <div class=""footer"">
            <p>&copy; {DateTime.UtcNow.Year} BluChilliWorks. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            try
            {
                await _emailService.SendEmailAsync(
                    request.EmailAddress,
                    "BluChilliWorks - Email Verification",
                    emailBody,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send verification email: {ex.Message}");
                return new ApiResult<string>(string.Empty, false, "ResendVerification.EmailFailed", "Failed to send verification email. Please try again later.");
            }

            return new ApiResult<string>(user.ID, true, "ResendVerification.Success", "Verification email has been sent. Please check your inbox.");
        }
    }
}

public class ResendVerificationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users/resend-verification", async (ResendVerificationRequest request, ISender sender) =>
        {
            var result = await sender.Send(request.Adapt<ResendVerification.Command>());
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AllowAnonymous(); // Allow anonymous access for resending verification
    }
}
