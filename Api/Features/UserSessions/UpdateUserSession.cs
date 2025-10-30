using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Extensions;
using Shared.Models;

namespace Api.Features.UserSessions;

public static class UpdateUserSession
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string Id { get; set; }
        public string? UserId { get; set; }
        public string? SessionKey { get; set; }
        public string? LastAccessedOn { get; set; }
        public bool IsExpired { get; set; }
        public DateTime ModifiedOn { get; set; } = DateTimeExtension.GetSouthAfricanTime();
        public string ModifiedBy { get; set; } = "system";
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.UserId).NotEmpty().WithMessage("Please ensure that you have entered your User Session {PropertyName}");
            RuleFor(c => c.SessionKey).NotEmpty().WithMessage("Please ensure that you have entered your User Session {PropertyName}");
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
                return new ApiResult<string>(string.Empty, false, "UpdateUserSession.Validation", errors);
            }

            var user = await _mongoDbRepository.Get<User>(request.UserId);
            if (user == null)
            {
                return new ApiResult<string>(string.Empty, false, "UpdateUserSession.UserNotFound", "The specified user was not found");
            }

            var record = await _userSessionRepository.GetUserSessionBySessionKeyAsync(request.SessionKey);
            if (record == null)
            {
                return new ApiResult<string>(string.Empty, false, "UpdateUserSession.UserSessionNotFound", "The specified user session was not found");
            }

            // Direct property assignment
            record.LastAccessedOn = request.LastAccessedOn;
            record.IsExpired = request.IsExpired;
            record.ModifiedBy = request.ModifiedBy;
            record.ModifiedOn = request.ModifiedOn.ToString("o");

            await _userSessionRepository.SaveUserSessionAsync(record);

            return new ApiResult<string>(record.ID, true);
        }
    }
}

public class UpdateUserSessionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("usersessions", async (UpdateUserSessionRequest request, ISender sender) =>
        {
            // Inline command creation for reduced stack usage
            var result = await sender.Send(request.Adapt<UpdateUserSession.Command>());
            return Results.Ok(result);
        })
        .WithTags("User Sessions")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
