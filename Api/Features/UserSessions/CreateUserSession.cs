using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Extensions;
using Shared.Models;
using System.Security;

namespace Api.Features.UserSessions;

public static class CreateUserSession
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string? UserId { get; set; }
        public string? Password { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "system";
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.UserId).NotEmpty().WithMessage("Please ensure that you have entered your User Session {PropertyName}");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ApiResult<string>>
    {
        private readonly IMongoDbRepository _mongoDbRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IValidator<Command> _validator;

        public Handler(IMongoDbRepository mongoDbRepository, IUserSessionRepository userSessionRepository, IValidator<Command> validator)
        {
            _mongoDbRepository = mongoDbRepository;
            _userSessionRepository = userSessionRepository;
            _validator = validator;
        }

        public async Task<ApiResult<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                // Aggregate error messages for clarity and efficiency
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ApiResult<string>(string.Empty, false, "CreateUserSession.Validation", errors);
            }

            var user = await _mongoDbRepository.Get<User>(request.UserId);
            if (user == null)
            {
                return new ApiResult<string>(string.Empty, false, "CreateUserSession.UserNotFound", "The specified user was not found");
            }

            var ss = Authenticate(user, request.Password);
            if (ss.Length == 0)
            {
                return new ApiResult<string>(string.Empty, false, "CreateUserSession.AuthenticationFailed", "Authentication failed. Invalid credentials.");
            }

            // Use object initializer directly
            var record = new UserSession
            {
                ExpiresOn = DateTimeExtension.GetSouthAfricanTime().AddMinutes(30).ToString("o"),
                IdleDuration = 30, // set idle duration from configuration file later
                IsActive = true,
                IsExpired = false,
                LastAccessedOn = DateTimeExtension.GetSouthAfricanTime().ToString("o"),
                SessionToken = ss.ToPlain(),
                UserId = request.UserId,
                CreatedOn = request.CreatedOn.ToString("o"),
                CreatedBy = request.CreatedBy
            };

            await _userSessionRepository.SaveUserSessionAsync(record);

            return new ApiResult<string>(record.ID, true);
        }

        private static SecureString Authenticate(User user, string? password)
        {
            // Use string.Equals with Ordinal comparison for security
            if (string.IsNullOrEmpty(password) || !string.Equals(password, user.DecryptedPassword, StringComparison.Ordinal))
                return string.Empty.CreateSecure();

            var sessionKey = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
            return sessionKey.CreateSecure();
        }
    }
}

public class CreateUserSessionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("usersessions", async (CreateUserSessionRequest request, ISender sender) =>
        {
            // Inline command creation for reduced stack usage
            var result = await sender.Send(request.Adapt<CreateUserSession.Command>());
            return Results.Ok(result);
        })
        .WithTags("User Sessions")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
