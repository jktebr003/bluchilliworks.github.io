using MudBlazorWeb.Features.Users.Application;
using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

using Xunit;

namespace MudBlazorWeb.Features.Users.Tests;

public class UserHandlersTests
{
	// ---------------------------------------------------------------------------
	// FilterUsersQuery
	// ---------------------------------------------------------------------------

	[Fact]
	public async Task FilterUsersHandler_ShouldReturnEmptyList_WhenNoUsersMatch()
	{
		var repository = new StubUserRepository();
		var handler = new FilterUsersQuery.Handler(repository);

		var result = await handler.Handle(new FilterUsersQuery.Query("nobody@example.com"), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Empty(result.Value);
	}

	[Fact]
	public async Task FilterUsersHandler_ShouldReturnMatchingUsers_MappedToDtos()
	{
		var userId = Guid.NewGuid();
		var users = new List<User> { MakeUser(userId, "alice", UserType.Customer) };

		var repository = new StubUserRepository { FilterResult = users };
		var handler = new FilterUsersQuery.Handler(repository);

		var result = await handler.Handle(new FilterUsersQuery.Query("alice@example.com"), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Single(result.Value);
		Assert.Equal(userId, result.Value[0].Id);
		Assert.Equal("alice", result.Value[0].Username);
	}

	// ---------------------------------------------------------------------------
	// GetUserByEmailAddressQuery
	// ---------------------------------------------------------------------------

	[Fact]
	public async Task GetUserByEmailAddressHandler_ShouldReturnSuccess_WhenUserExists()
	{
		var userId = Guid.NewGuid();
		var user = MakeUser(userId, "bob", UserType.Customer);

		var repository = new StubUserRepository { GetByEmailResult = user };
		var handler = new GetUserByEmailAddressQuery.Handler(repository);

		var result = await handler.Handle(new GetUserByEmailAddressQuery.Query("bob@example.com"), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(userId, result.Value.Id);
		Assert.Equal("bob", result.Value.Username);
	}

	[Fact]
	public async Task GetUserByEmailAddressHandler_ShouldReturnNotFound_WhenUserIsNull()
	{
		var repository = new StubUserRepository { GetByEmailResult = null };
		var handler = new GetUserByEmailAddressQuery.Handler(repository);

		var result = await handler.Handle(new GetUserByEmailAddressQuery.Query("unknown@example.com"), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("GetUserByEmailAddress.NotFound", result.Code);
	}

	// ---------------------------------------------------------------------------
	// GetUserByIdQuery
	// ---------------------------------------------------------------------------

	[Fact]
	public async Task GetUserByIdHandler_ShouldReturnSuccess_WhenUserExists()
	{
		var userId = Guid.NewGuid();
		var user = MakeUser(userId, "carol", UserType.Staff);

		var repository = new StubUserRepository { GetByIdHandler = (_, _) => Task.FromResult(user) };
		var handler = new GetUserByIdQuery.Handler(repository);

		var result = await handler.Handle(new GetUserByIdQuery.Query(userId), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(userId, result.Value.Id);
		Assert.Equal("carol", result.Value.Username);
	}

	[Fact]
	public async Task GetUserByIdHandler_ShouldReturnNotFound_WhenUserDoesNotExist()
	{
		var repository = new StubUserRepository(); // GetByIdHandler throws by default
		var handler = new GetUserByIdQuery.Handler(repository);

		var result = await handler.Handle(new GetUserByIdQuery.Query(Guid.NewGuid()), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("GetUserById.NotFound", result.Code);
	}

	// ---------------------------------------------------------------------------
	// GetUsersQuery
	// ---------------------------------------------------------------------------

	[Fact]
	public async Task GetUsersHandler_ShouldReturnEmptyPagedResult_WhenNoUsersExist()
	{
		var repository = new StubUserRepository();
		var handler = new GetUsersQuery.Handler(repository);

		var result = await handler.Handle(new GetUsersQuery.Query(null, null), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Empty(result.Value);
		Assert.Equal(0, result.TotalItems);
		Assert.Equal(1, result.TotalPages);
	}

	[Fact]
	public async Task GetUsersHandler_ShouldReturnAllUsers_WhenNoPaginationProvided()
	{
		var users = new List<User>
		{
			MakeUser(Guid.NewGuid(), "dave", UserType.Customer),
			MakeUser(Guid.NewGuid(), "eve", UserType.Customer)
		};

		var repository = new StubUserRepository { GetAllResult = users };
		var handler = new GetUsersQuery.Handler(repository);

		var result = await handler.Handle(new GetUsersQuery.Query(null, null), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(2, result.TotalItems);
		Assert.Equal(2, result.Value.Count);
	}

	[Fact]
	public async Task GetUsersHandler_ShouldReturnFirstPage_WhenPaginationProvided()
	{
		var users = Enumerable.Range(1, 5)
			.Select(i => MakeUser(Guid.NewGuid(), $"user{i}", UserType.Customer))
			.ToList();

		var repository = new StubUserRepository { GetAllResult = users };
		var handler = new GetUsersQuery.Handler(repository);

		var result = await handler.Handle(new GetUsersQuery.Query(PageSize: 2, PageNumber: 1), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(5, result.TotalItems);
		Assert.Equal(3, result.TotalPages);
		Assert.Equal(2, result.Value.Count);
	}

	[Fact]
	public async Task GetUsersHandler_ShouldReturnEmptyPage_WhenPageNumberExceedsTotalPages()
	{
		var users = new List<User>
		{
			MakeUser(Guid.NewGuid(), "frank", UserType.Customer)
		};

		var repository = new StubUserRepository { GetAllResult = users };
		var handler = new GetUsersQuery.Handler(repository);

		var result = await handler.Handle(new GetUsersQuery.Query(PageSize: 1, PageNumber: 99), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Empty(result.Value);
	}

	// ---------------------------------------------------------------------------
	// SearchUserQuery
	// ---------------------------------------------------------------------------

	[Fact]
	public async Task SearchUserHandler_ShouldReturnResults_WhenMatchesExist()
	{
		var users = new List<User>
		{
			MakeUser(Guid.NewGuid(), "grace", UserType.Customer),
			MakeUser(Guid.NewGuid(), "henry", UserType.Staff)
		};

		var repository = new StubUserRepository { SearchResult = users };
		var handler = new SearchUserQuery.Handler(repository);

		var query = new SearchUserQuery.Query("grace", null, null, null, null, null, null, null, null);
		var result = await handler.Handle(query, CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(2, result.TotalItems);
		Assert.Equal(2, result.Value.Count);
	}

	[Fact]
	public async Task SearchUserHandler_ShouldReturnEmptyResult_WhenNoMatchesFound()
	{
		var repository = new StubUserRepository { SearchResult = [] };
		var handler = new SearchUserQuery.Handler(repository);

		var query = new SearchUserQuery.Query("nobody", null, null, null, null, null, null, null, null);
		var result = await handler.Handle(query, CancellationToken.None);

		Assert.True(result.Success);
		Assert.Empty(result.Value);
		Assert.Equal(0, result.TotalItems);
	}

	[Fact]
	public async Task SearchUserHandler_ShouldApplyPagination_ToFilteredResults()
	{
		var users = Enumerable.Range(1, 6)
			.Select(i => MakeUser(Guid.NewGuid(), $"user{i}", UserType.Customer))
			.ToList();

		var repository = new StubUserRepository { SearchResult = users };
		var handler = new SearchUserQuery.Handler(repository);

		var query = new SearchUserQuery.Query(null, null, null, null, null, null, null, PageSize: 2, PageNumber: 2);
		var result = await handler.Handle(query, CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(6, result.TotalItems);
		Assert.Equal(3, result.TotalPages);
		Assert.Equal(2, result.Value.Count);
	}

	// ---------------------------------------------------------------------------
	// CreateUserCommand
	// ---------------------------------------------------------------------------

	[Theory]
	[InlineData("", "username", "First", "Last")]
	[InlineData("email@example.com", "", "First", "Last")]
	[InlineData("email@example.com", "username", "", "Last")]
	[InlineData("email@example.com", "username", "First", "")]
	public async Task CreateUserHandler_ShouldReturnValidationFailure_WhenRequiredFieldIsMissing(
		string email, string username, string firstName, string lastName)
	{
		var repository = new StubUserRepository();
		var handler = new CreateUserCommand.Handler(repository);

		var request = new CreateUserRequest
		{
			EmailAddress = email,
			Username = username,
			FirstName = firstName,
			LastName = lastName
		};

		var result = await handler.Handle(new CreateUserCommand.Command(request), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("CreateUser.Validation", result.Code);
	}

	[Fact]
	public async Task CreateUserHandler_ShouldReturnUserExists_WhenEmailAlreadyRegistered()
	{
		var existingUser = MakeUser(Guid.NewGuid(), "existing", UserType.Customer);
		var repository = new StubUserRepository { GetByEmailResult = existingUser };
		var handler = new CreateUserCommand.Handler(repository);

		var request = new CreateUserRequest
		{
			EmailAddress = "existing@example.com",
			Username = "existing",
			FirstName = "First",
			LastName = "Last"
		};

		var result = await handler.Handle(new CreateUserCommand.Command(request), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("CreateUser.UserExists", result.Code);
	}

	[Fact]
	public async Task CreateUserHandler_ShouldCreateUser_AndReturnNewId()
	{
		var repository = new StubUserRepository { GetByEmailResult = null };
		var handler = new CreateUserCommand.Handler(repository);

		var request = new CreateUserRequest
		{
			EmailAddress = "new@example.com",
			Username = "newuser",
			FirstName = "New",
			LastName = "User"
		};

		var result = await handler.Handle(new CreateUserCommand.Command(request), CancellationToken.None);

		Assert.True(result.Success);
		Assert.True(Guid.TryParse(result.Value, out _));
		Assert.NotNull(repository.SavedUser);
		Assert.Equal("new@example.com", repository.SavedUser!.EmailAddress);
	}

	[Fact]
	public async Task CreateUserHandler_ShouldAutoGenerateName_WhenNameIsNotProvided()
	{
		var repository = new StubUserRepository { GetByEmailResult = null };
		var handler = new CreateUserCommand.Handler(repository);

		var request = new CreateUserRequest
		{
			EmailAddress = "auto@example.com",
			Username = "autoname",
			FirstName = "Auto",
			LastName = "Name"
		};

		await handler.Handle(new CreateUserCommand.Command(request), CancellationToken.None);

		Assert.Equal("Auto Name", repository.SavedUser!.Name);
	}

	// ---------------------------------------------------------------------------
	// UpdateUserCommand
	// ---------------------------------------------------------------------------

	[Theory]
	[InlineData("", "username")]
	[InlineData("email@example.com", "")]
	public async Task UpdateUserHandler_ShouldReturnValidationFailure_WhenRequiredFieldIsMissing(
		string email, string username)
	{
		var repository = new StubUserRepository();
		var handler = new UpdateUserCommand.Handler(repository);

		var request = new UpdateUserRequest
		{
			Id = Guid.NewGuid(),
			EmailAddress = email,
			Username = username
		};

		var result = await handler.Handle(new UpdateUserCommand.Command(request), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("UpdateUser.Validation", result.Code);
	}

	[Fact]
	public async Task UpdateUserHandler_ShouldReturnNotFound_WhenUserDoesNotExist()
	{
		var repository = new StubUserRepository(); // throws by default
		var handler = new UpdateUserCommand.Handler(repository);

		var request = new UpdateUserRequest
		{
			Id = Guid.NewGuid(),
			EmailAddress = "missing@example.com",
			Username = "missing"
		};

		var result = await handler.Handle(new UpdateUserCommand.Command(request), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("UpdateUser.NotFound", result.Code);
	}

	[Fact]
	public async Task UpdateUserHandler_ShouldUpdateUser_AndReturnId()
	{
		var userId = Guid.NewGuid();
		var existingUser = MakeUser(userId, "oldusername", UserType.Customer);

		var repository = new StubUserRepository
		{
			GetByIdHandler = (_, _) => Task.FromResult(existingUser)
		};
		var handler = new UpdateUserCommand.Handler(repository);

		var request = new UpdateUserRequest
		{
			Id = userId,
			EmailAddress = "updated@example.com",
			Username = "newusername",
			FirstName = "Updated",
			LastName = "User",
			UserType = UserType.Staff
		};

		var result = await handler.Handle(new UpdateUserCommand.Command(request), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(userId.ToString(), result.Value);
		Assert.NotNull(repository.UpdatedUser);
		Assert.Equal("updated@example.com", repository.UpdatedUser!.EmailAddress);
		Assert.Equal("newusername", repository.UpdatedUser.Username);
		Assert.Equal((int)UserType.Staff, repository.UpdatedUser.UserType);
	}

	[Fact]
	public async Task UpdateUserHandler_ShouldMapJobs_WhenJobsProvided()
	{
		var userId = Guid.NewGuid();
		var existingUser = MakeUser(userId, "jobuser", UserType.Customer);

		var repository = new StubUserRepository
		{
			GetByIdHandler = (_, _) => Task.FromResult(existingUser)
		};
		var handler = new UpdateUserCommand.Handler(repository);

		var request = new UpdateUserRequest
		{
			Id = userId,
			EmailAddress = "job@example.com",
			Username = "jobuser",
			UserType = UserType.Customer,
			Jobs =
			[
				new JobResponse { Company = "Acme", Position = "Dev", StartDate = "2020-01", EndDate = "2022-01", Responsibilities = "Code" }
			]
		};

		var result = await handler.Handle(new UpdateUserCommand.Command(request), CancellationToken.None);

		Assert.True(result.Success);
		Assert.NotNull(repository.UpdatedUser!.Jobs);
		Assert.Single(repository.UpdatedUser.Jobs!);
		Assert.Equal("Acme", repository.UpdatedUser.Jobs![0].Company);
	}

	// ---------------------------------------------------------------------------
	// Helpers
	// ---------------------------------------------------------------------------

	private static User MakeUser(Guid id, string username, UserType userType) => new()
	{
		Id = id,
		Name = username,
		FirstName = username,
		LastName = "L",
		Username = username,
		EmailAddress = $"{username}@example.com",
		HashedPassword = "hash",
		UserType = (int)userType,
		CreatedOn = DateTime.UtcNow,
		CreatedBy = "test"
	};

	private sealed class StubUserRepository : IUserRepository
	{
		public List<User> FilterResult { get; init; } = [];
		public User? GetByEmailResult { get; init; }
		public List<User> GetAllResult { get; init; } = [];
		public List<User> SearchResult { get; init; } = [];
		public Func<Guid, CancellationToken, Task<User>>? GetByIdHandler { get; init; }
		public User? SavedUser { get; private set; }
		public User? UpdatedUser { get; private set; }

		public Task<List<User>> FilterUsersByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default)
			=> Task.FromResult(FilterResult);

		public Task<User?> GetUserByEmailAddressAsync(string? emailAddress, CancellationToken cancellationToken = default)
			=> Task.FromResult(GetByEmailResult);

		public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult(GetAllResult);

		public Task<User> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
			=> GetByIdHandler?.Invoke(id, cancellationToken)
			   ?? Task.FromException<User>(new InvalidOperationException("not found"));

		public Task SaveUserAsync(User user, CancellationToken cancellationToken = default)
		{
			SavedUser = user;
			return Task.CompletedTask;
		}

		public Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
		{
			UpdatedUser = user;
			return Task.CompletedTask;
		}

		public Task<List<User>> SearchUsersAsync(string? search = null, UserType? role = null, string? gender = null,
			string? package = null, bool? emailVerified = null, DateTime? dobFrom = null, DateTime? dobTo = null,
			CancellationToken cancellationToken = default)
			=> Task.FromResult(SearchResult);
	}
}
