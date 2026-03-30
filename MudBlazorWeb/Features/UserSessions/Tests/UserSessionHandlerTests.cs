using MudBlazorWeb.Features.UserSessions.Application;
using MudBlazorWeb.Features.UserSessions.Domain;
using MudBlazorWeb.Shared.Models;

using Xunit;

namespace MudBlazorWeb.Features.UserSessions.Tests;

public class UserSessionHandlerTests
{
	[Fact]
	public async Task GetUserSessionHandler_ShouldReturnValidationFailure_WhenIdIsInvalid()
	{
		var repository = new FakeUserSessionRepository();
		var handler = new GetUserSessionQuery.Handler(repository);

		var result = await handler.Handle(new GetUserSessionQuery.Query("invalid-guid"), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("GetUserSession.Validation", result.Code);
	}

	[Fact]
	public async Task GetUserSessionHandler_ShouldReturnNotFound_WhenSessionDoesNotExist()
	{
		var repository = new FakeUserSessionRepository
		{
			GetByIdHandler = _ => throw new InvalidOperationException("not found")
		};
		var handler = new GetUserSessionQuery.Handler(repository);

		var result = await handler.Handle(new GetUserSessionQuery.Query(Guid.NewGuid().ToString()), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("GetUserSession.Null", result.Code);
	}

	[Fact]
	public async Task GetUserSessionBySessionTokenHandler_ShouldReturnSuccess_WhenSessionExists()
	{
		var session = CreateSession(Guid.NewGuid(), "user-1", "token-1");
		var repository = new FakeUserSessionRepository
		{
			GetByTokenHandler = _ => session
		};
		var handler = new GetUserSessionBySessionTokenQuery.Handler(repository);

		var result = await handler.Handle(new GetUserSessionBySessionTokenQuery.Query("token-1"), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(session.Id, result.Value.Id);
	}

	[Fact]
	public async Task GetUserSessionsHandler_ShouldReturnPagedData_WhenPagingParametersAreValid()
	{
		var sessions = new List<UserSession>
		{
			CreateSession(Guid.NewGuid(), "u1", "t1"),
			CreateSession(Guid.NewGuid(), "u2", "t2"),
			CreateSession(Guid.NewGuid(), "u3", "t3")
		};

		var repository = new FakeUserSessionRepository { GetAllResult = sessions };
		var handler = new GetUserSessionsQuery.Handler(repository);

		var result = await handler.Handle(new GetUserSessionsQuery.Query(2, 2), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(2, result.TotalPages);
		Assert.Equal(3, result.TotalItems);
		Assert.Single(result.Value);
		Assert.Equal(sessions[2].Id, result.Value[0].Id);
	}

	[Fact]
	public async Task FilterUserSessionsHandler_ShouldReturnFilteredItems()
	{
		var expected = new List<UserSession> { CreateSession(Guid.NewGuid(), "target-user", "token") };
		var repository = new FakeUserSessionRepository
		{
			FilterByUserIdHandler = id =>
			{
				Assert.Equal("target-user", id);
				return expected;
			}
		};
		var handler = new FilterUserSessionsQuery.Handler(repository);

		var result = await handler.Handle(new FilterUserSessionsQuery.Query("target-user"), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Single(result.Value);
		Assert.Equal(expected[0].Id, result.Value[0].Id);
	}

	[Fact]
	public async Task CreateUserSessionHandler_ShouldReturnValidationFailure_WhenUserIdMissing()
	{
		var repository = new FakeUserSessionRepository();
		var handler = new CreateUserSessionCommand.Handler(repository);

		var result = await handler.Handle(new CreateUserSessionCommand.Command(new CreateUserSessionRequest { UserId = "" }), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("CreateUserSession.Validation", result.Code);
	}

	[Fact]
	public async Task CreateUserSessionHandler_ShouldPersistSession_WhenRequestValid()
	{
		var repository = new FakeUserSessionRepository();
		var handler = new CreateUserSessionCommand.Handler(repository);

		var result = await handler.Handle(new CreateUserSessionCommand.Command(new CreateUserSessionRequest
		{
			UserId = "user-1",
			CreatedBy = "tester"
		}), CancellationToken.None);

		Assert.True(result.Success);
		Assert.NotEmpty(result.Value);
		Assert.NotNull(repository.SavedSession);
		Assert.Equal("user-1", repository.SavedSession!.UserId);
		Assert.Equal("tester", repository.SavedSession.CreatedBy);
	}

	[Fact]
	public async Task UpdateUserSessionHandler_ShouldReturnValidationFailure_WhenRequiredFieldsMissing()
	{
		var repository = new FakeUserSessionRepository();
		var handler = new UpdateUserSessionCommand.Handler(repository);

		var result = await handler.Handle(new UpdateUserSessionCommand.Command(new UpdateUserSessionRequest
		{
			UserId = "",
			SessionToken = ""
		}), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("UpdateUserSession.Validation", result.Code);
	}

	[Fact]
	public async Task UpdateUserSessionHandler_ShouldReturnNotFound_WhenTokenDoesNotExist()
	{
		var repository = new FakeUserSessionRepository
		{
			GetByTokenHandler = _ => throw new InvalidOperationException("not found")
		};
		var handler = new UpdateUserSessionCommand.Handler(repository);

		var result = await handler.Handle(new UpdateUserSessionCommand.Command(new UpdateUserSessionRequest
		{
			UserId = "user-1",
			SessionToken = "missing-token"
		}), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("UpdateUserSession.NotFound", result.Code);
	}

	[Fact]
	public async Task UpdateUserSessionHandler_ShouldUpdateSession_WhenRequestValid()
	{
		var existing = CreateSession(Guid.NewGuid(), "user-1", "token-1");
		var repository = new FakeUserSessionRepository
		{
			GetByTokenHandler = _ => existing
		};
		var handler = new UpdateUserSessionCommand.Handler(repository);

		var result = await handler.Handle(new UpdateUserSessionCommand.Command(new UpdateUserSessionRequest
		{
			UserId = "user-1",
			SessionToken = "token-1",
			LastAccessedOn = new DateTimeOffset(2026, 3, 24, 0, 0, 0, TimeSpan.Zero),
			IsExpired = true,
			ModifiedBy = "editor",
			ModifiedOn = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc)
		}), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(existing.Id.ToString(), result.Value);
		Assert.NotNull(repository.UpdatedSession);
		Assert.True(repository.UpdatedSession!.IsExpired);
		Assert.Equal("editor", repository.UpdatedSession.ModifiedBy);
	}

	private static UserSession CreateSession(Guid id, string userId, string token)
	{
		var now = DateTimeOffset.UtcNow;

		return new UserSession
		{
			Id = id,
			UserId = userId,
			SessionTokenHash = token,
			IdleDuration = 30,
			LastAccessedOn = now,
			ExpiresOn = now.AddMinutes(30),
			AbsoluteExpiresOn = now.AddHours(8),
			UserStateVersion = "test-version",
			IsExpired = false,
			IsActive = true,
			CreatedOn = DateTime.UtcNow,
			CreatedBy = "test"
		};
	}

	private sealed class FakeUserSessionRepository : IUserSessionRepository
	{
		public Func<string, List<UserSession>>? FilterByUserIdHandler { get; init; }
		public Func<string, UserSession>? GetByIdHandler { get; init; }
		public Func<string, UserSession>? GetByTokenHandler { get; init; }
		public List<UserSession> GetAllResult { get; init; } = [];

		public UserSession? SavedSession { get; private set; }
		public UserSession? UpdatedSession { get; private set; }

		public Task<List<UserSession>> FilterUserSessionsByUserIdAsync(string userId)
			=> Task.FromResult(FilterByUserIdHandler?.Invoke(userId) ?? []);

		public Task<List<UserSession>> GetAllUserSessionsAsync()
			=> Task.FromResult(GetAllResult);

		public Task<UserSession> GetUserSessionByIdAsync(string id)
		{
			if (!Guid.TryParse(id, out _))
			{
				throw new ArgumentException("User session ID must be a valid GUID.", nameof(id));
			}

			if (GetByIdHandler != null)
			{
				return Task.FromResult(GetByIdHandler.Invoke(id));
			}

			throw new InvalidOperationException("not found");
		}

		public Task<UserSession> GetUserSessionBySessionTokenAsync(string sessionToken)
		{
			if (GetByTokenHandler != null)
			{
				return Task.FromResult(GetByTokenHandler.Invoke(sessionToken));
			}

			throw new InvalidOperationException("not found");
		}

		public Task<UserSession?> FindUserSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			if (GetByIdHandler == null)
			{
				return Task.FromResult<UserSession?>(null);
			}

			return Task.FromResult<UserSession?>(GetByIdHandler.Invoke(id.ToString()));
		}

		public Task<UserSession?> FindUserSessionByTokenHashAsync(string sessionTokenHash, CancellationToken cancellationToken = default)
		{
			if (GetByTokenHandler == null)
			{
				return Task.FromResult<UserSession?>(null);
			}

			return Task.FromResult<UserSession?>(GetByTokenHandler.Invoke(sessionTokenHash));
		}

		public Task SaveUserSessionAsync(UserSession userSession)
		{
			SavedSession = userSession;
			return Task.CompletedTask;
		}

		public Task UpdateUserSessionAsync(UserSession userSession)
		{
			UpdatedSession = userSession;
			return Task.CompletedTask;
		}
	}
}
