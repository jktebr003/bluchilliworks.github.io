using Carter;

using MediatR;

using MudBlazorWeb.Features.Authentication.Infrastructure;
using MudBlazorWeb.Features.UserSessions.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.UserSessions.Application;

public static class CreateUserSessionCommand
{
	public record Command(CreateUserSessionRequest Request) : IRequest<Result<string>>;

	internal sealed class Handler : IRequestHandler<Command, Result<string>>
	{
		private readonly IUserSessionRepository _userSessionRepository;

		public Handler(IUserSessionRepository userSessionRepository)
		{
			_userSessionRepository = userSessionRepository;
		}

		public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.Request.UserId))
			{
				return new Result<string>(string.Empty, false, "CreateUserSession.Validation", "Please provide a user ID.");
			}

			var now = DateTime.UtcNow;
			var nowOffset = DateTimeOffset.UtcNow;
			var rawSessionToken = $"manual-{Guid.NewGuid():N}";
			var userSession = new UserSession
			{
				Id = Guid.NewGuid(),
				UserId = request.Request.UserId,
				SessionTokenHash = SessionTokenHasher.HashToken(rawSessionToken),
				IdleDuration = 30,
				LastAccessedOn = nowOffset,
				ExpiresOn = nowOffset.AddMinutes(30),
				AbsoluteExpiresOn = nowOffset.AddHours(8),
				UserStateVersion = "manual-session",
				IsExpired = false,
				IsActive = true,
				CreatedOn = request.Request.CreatedOn == default ? now : request.Request.CreatedOn,
				CreatedBy = request.Request.CreatedBy
			};

			await _userSessionRepository.SaveUserSessionAsync(userSession);

			return new Result<string>(userSession.Id.ToString(), true);
		}
	}
}

public static class UpdateUserSessionCommand
{
	public record Command(UpdateUserSessionRequest Request) : IRequest<Result<string>>;

	internal sealed class Handler : IRequestHandler<Command, Result<string>>
	{
		private readonly IUserSessionRepository _userSessionRepository;

		public Handler(IUserSessionRepository userSessionRepository)
		{
			_userSessionRepository = userSessionRepository;
		}

		public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.Request.UserId) ||
				string.IsNullOrWhiteSpace(request.Request.SessionToken))
			{
				return new Result<string>(string.Empty, false, "UpdateUserSession.Validation", "Please provide user ID and session token.");
			}

			UserSession existingSession;
			try
			{
				existingSession = await _userSessionRepository.GetUserSessionBySessionTokenAsync(request.Request.SessionToken);
			}
			catch (InvalidOperationException)
			{
				return new Result<string>(string.Empty, false, "UpdateUserSession.NotFound", "The user session with the specified session token was not found.");
			}

			existingSession.UserId = request.Request.UserId;
			existingSession.LastAccessedOn = request.Request.LastAccessedOn ?? existingSession.LastAccessedOn;
			existingSession.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(existingSession.IdleDuration);
			existingSession.IsExpired = request.Request.IsExpired;
			existingSession.ModifiedOn = request.Request.ModifiedOn;
			existingSession.ModifiedBy = request.Request.ModifiedBy;

			await _userSessionRepository.UpdateUserSessionAsync(existingSession);

			return new Result<string>(existingSession.Id.ToString(), true);
		}
	}
}

public class UserSessionCommandsEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("api/usersessions", async (CreateUserSessionRequest request, ISender sender) =>
		{
			var command = new CreateUserSessionCommand.Command(request);
			var result = await sender.Send(command);

			return Results.Ok(result);
		}).WithTags("UserSessions")
		  .AddEndpointFilter<AuthenticationFilter>();

		app.MapPut("api/usersessions", async (UpdateUserSessionRequest request, ISender sender) =>
		{
			var command = new UpdateUserSessionCommand.Command(request);
			var result = await sender.Send(command);

			return Results.Ok(result);
		}).WithTags("UserSessions")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
