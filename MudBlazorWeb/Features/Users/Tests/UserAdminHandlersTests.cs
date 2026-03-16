using MudBlazorWeb.Features.Users.Application;
using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Shared.Enums;

using Xunit;

namespace MudBlazorWeb.Features.Users.Tests;

public class UserAdminHandlersTests
{
    [Fact]
    public async Task GetUserDetailsHandler_ShouldReturnSuccess_WhenRequesterIsStaff()
    {
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var repository = new FakeUserRepository
        {
            GetByIdHandler = (id, _) =>
            {
                if (id == requesterId)
                {
                    return Task.FromResult(CreateUser(requesterId, "staff", UserType.Staff));
                }

                return Task.FromResult(CreateUser(targetId, "target", UserType.Customer));
            }
        };

        var handler = new GetUserDetailsQuery.Handler(repository);

        var result = await handler.Handle(new GetUserDetailsQuery.Query(targetId, requesterId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(targetId.ToString(), result.Value.ID);
        Assert.Equal("target", result.Value.Username);
    }

    [Fact]
    public async Task GetUserDetailsHandler_ShouldReturnUnauthorized_WhenRequesterIsNotStaff()
    {
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var repository = new FakeUserRepository
        {
            GetByIdHandler = (id, _) =>
            {
                if (id == requesterId)
                {
                    return Task.FromResult(CreateUser(requesterId, "customer", UserType.Customer));
                }

                return Task.FromResult(CreateUser(targetId, "target", UserType.Customer));
            }
        };

        var handler = new GetUserDetailsQuery.Handler(repository);

        var result = await handler.Handle(new GetUserDetailsQuery.Query(targetId, requesterId), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("GetUserDetails.Unauthorized", result.Code);
    }

    [Fact]
    public async Task ChangeUserPasswordHandler_ShouldReturnValidationFailure_ForWeakPassword()
    {
        var repository = new FakeUserRepository();
        var handler = new ChangeUserPasswordCommand.Handler(repository);

        var result = await handler.Handle(new ChangeUserPasswordCommand.Command(Guid.NewGuid(), "weak", null), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ChangeUserPassword.Validation", result.Code);
    }

    [Fact]
    public async Task ChangeUserPasswordHandler_ShouldUpdateHashedPassword_WhenValid()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, "target", UserType.Customer);
        user.HashedPassword = "old";

        var repository = new FakeUserRepository
        {
            GetByIdHandler = (_, _) => Task.FromResult(user)
        };

        var handler = new ChangeUserPasswordCommand.Handler(repository);

        var result = await handler.Handle(new ChangeUserPasswordCommand.Command(userId, "Abcdef1!", null), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(repository.UpdatedUser);
        Assert.NotEqual("old", repository.UpdatedUser!.HashedPassword);
        Assert.Equal(64, repository.UpdatedUser.HashedPassword.Length);
    }

    [Fact]
    public async Task ChangeUserRoleHandler_ShouldUpdateRole_WhenRequesterIsStaff()
    {
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var repository = new FakeUserRepository
        {
            GetByIdHandler = (id, _) =>
            {
                if (id == requesterId)
                {
                    return Task.FromResult(CreateUser(requesterId, "staff", UserType.Staff));
                }

                return Task.FromResult(CreateUser(targetId, "target", UserType.Customer));
            }
        };

        var handler = new ChangeUserRoleCommand.Handler(repository);

        var result = await handler.Handle(new ChangeUserRoleCommand.Command(targetId, UserType.Staff, requesterId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(repository.UpdatedUser);
        Assert.Equal((int)UserType.Staff, repository.UpdatedUser!.UserType);
    }

    [Fact]
    public async Task ChangeUserRoleHandler_ShouldReturnUnauthorized_WhenRequesterIsNotStaff()
    {
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var repository = new FakeUserRepository
        {
            GetByIdHandler = (id, _) =>
            {
                if (id == requesterId)
                {
                    return Task.FromResult(CreateUser(requesterId, "customer", UserType.Customer));
                }

                return Task.FromResult(CreateUser(targetId, "target", UserType.Customer));
            }
        };

        var handler = new ChangeUserRoleCommand.Handler(repository);

        var result = await handler.Handle(new ChangeUserRoleCommand.Command(targetId, UserType.Staff, requesterId), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ChangeUserRole.Unauthorized", result.Code);
    }

    private static User CreateUser(Guid id, string username, UserType userType)
    {
        return new User
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
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public Func<Guid, CancellationToken, Task<User>>? GetByIdHandler { get; init; }
        public User? UpdatedUser { get; private set; }

        public Task<List<User>> FilterUsersByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default)
            => Task.FromResult(new List<User>());

        public Task<User?> GetUserByEmailAddressAsync(string? emailAddress, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(null);

        public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new List<User>());

        public Task<User> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => GetByIdHandler?.Invoke(id, cancellationToken)
               ?? Task.FromException<User>(new InvalidOperationException("not found"));

        public Task SaveUserAsync(User user, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            UpdatedUser = user;
            return Task.CompletedTask;
        }

        public Task<List<User>> SearchUsersAsync(string? search = null, UserType? role = null, string? gender = null, string? package = null, bool? emailVerified = null, DateTime? dobFrom = null, DateTime? dobTo = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new List<User>());
    }
}