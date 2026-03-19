using Fluxor;
using MediatR;

using MudBlazorWeb.Features.Users.Application;
using MudBlazorWeb.Features.Users.UI;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;
using MudBlazorWeb.Shared.Services;

using Xunit;

using static MudBlazorWeb.Features.Users.UI.UsersActions;

namespace MudBlazorWeb.Features.Users.Tests;

public class UsersEffectsTests
{
    [Fact]
    public async Task HandleLoadUsers_ShouldDispatchSuccess_WhenMediatorReturnsPagedUsers()
    {
        object? capturedRequest = null;
        var mediator = new FakeMediator
        {
            SendHandler = request =>
            {
                capturedRequest = request;
                var dto = CreateGetUsersDto("alice");
                return Task.FromResult<object?>(new PagedResult<List<GetUsersQuery.UserDto>>(
                    new List<GetUsersQuery.UserDto> { dto }, true, 1, 1, 1, 10));
            }
        };

        var effects = new UsersEffects(mediator, new FakeCurrentUserContext(), new StaticUsersState(new UsersState()));
        var dispatcher = new FakeDispatcher();

        await effects.HandleLoadUsers(new LoadUsersAction(1, 10), dispatcher);

        var query = Assert.IsType<GetUsersQuery.Query>(capturedRequest);
        Assert.Equal(10, query.PageSize);
        Assert.Equal(1, query.PageNumber);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var success = Assert.IsType<LoadUsersSuccessAction>(action);
        Assert.Single(success.Users);
        Assert.Equal("alice", success.Users[0].Username);
    }

    [Fact]
    public async Task HandleLoadUserProfile_ShouldDispatchFailure_WhenUserIdIsInvalid()
    {
        var effects = new UsersEffects(new FakeMediator(), new FakeCurrentUserContext(), new StaticUsersState(new UsersState()));
        var dispatcher = new FakeDispatcher();

        await effects.HandleLoadUserProfile(new LoadUserProfileAction("bad-guid"), dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadUserProfileFailedAction>(action);
        Assert.Equal("Invalid user ID.", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleUpdateUserProfile_ShouldDispatchSuccess_AndUpdateAuthUser_WhenMediatorSucceeds()
    {
        var userId = Guid.NewGuid();
        var auth = new FakeCurrentUserContext();

        var mediator = new FakeMediator
        {
            SendHandler = request =>
            {
                return request switch
                {
                    UpdateUserCommand.Command => Task.FromResult<object?>(new Result<string>(userId.ToString(), true)),
                    GetUserByIdQuery.Query => Task.FromResult<object?>(new Result<GetUserByIdQuery.UserDto>(CreateGetUserByIdDto(userId, "updated"), true)),
                    _ => throw new InvalidOperationException("Unexpected request")
                };
            }
        };

        var request = new UpdateUserRequest
        {
            Id = userId,
            Username = "updated",
            EmailAddress = "u@example.com",
            UserType = UserType.Customer
        };

        var effects = new UsersEffects(mediator, auth, new StaticUsersState(new UsersState()));
        var dispatcher = new FakeDispatcher();

        await effects.HandleUpdateUserProfile(new UpdateUserProfileAction(request), dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var success = Assert.IsType<UpdateUserProfileSuccessAction>(action);
        Assert.Equal("updated", success.User.Username);
        Assert.NotNull(auth.UpdatedUser);
        Assert.Equal("updated", auth.UpdatedUser!.Username);
    }

    [Fact]
    public async Task HandleChangeUserRole_ShouldDispatchSuccess_AndReloadDetails_WhenMediatorSucceeds()
    {
        var userId = Guid.NewGuid().ToString();
        var requesterId = Guid.NewGuid().ToString();

        var mediator = new FakeMediator
        {
            SendHandler = request => request switch
            {
                ChangeUserRoleCommand.Command => Task.FromResult<object?>(new Result<string>("ok", true)),
                _ => throw new InvalidOperationException("Unexpected request")
            }
        };

        var effects = new UsersEffects(mediator, new FakeCurrentUserContext(), new StaticUsersState(new UsersState()));
        var dispatcher = new FakeDispatcher();

        await effects.HandleChangeUserRole(new ChangeUserRoleAction(userId, UserType.Staff, requesterId), dispatcher);

        Assert.Equal(2, dispatcher.DispatchedActions.Count);
        Assert.IsType<ChangeUserRoleSuccessAction>(dispatcher.DispatchedActions[0]);
        var reload = Assert.IsType<LoadUserDetailsAction>(dispatcher.DispatchedActions[1]);
        Assert.Equal(userId, reload.UserId);
        Assert.Equal(requesterId, reload.RequestingUserId);
    }

    [Fact]
    public async Task HandleSearchUsers_ShouldDispatchSuccess_WhenMediatorReturnsPagedResult()
    {
        var state = new UsersState
        {
            SearchQuery = "ali",
            RoleFilter = UserType.Customer,
            IsSearchActive = true
        };

        object? capturedRequest = null;
        var mediator = new FakeMediator
        {
            SendHandler = request =>
            {
                capturedRequest = request;
                var dto = CreateSearchDto("alice");
                return Task.FromResult<object?>(new PagedResult<List<SearchUserQuery.UserDto>>(
                    new List<SearchUserQuery.UserDto> { dto }, true, 1, 1, 1, 10));
            }
        };

        var effects = new UsersEffects(mediator, new FakeCurrentUserContext(), new StaticUsersState(state));
        var dispatcher = new FakeDispatcher();

        await effects.HandleSearchUsers(new SearchUsersAction(1, 10), dispatcher);

        var query = Assert.IsType<SearchUserQuery.Query>(capturedRequest);
        Assert.Equal("ali", query.Search);
        Assert.Equal(UserType.Customer, query.Role);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var success = Assert.IsType<SearchUsersSuccessAction>(action);
        Assert.Single(success.Users);
        Assert.Equal("alice", success.Users[0].Username);
    }

    private static GetUsersQuery.UserDto CreateGetUsersDto(string username)
        => new(
            Name: "Alice",
            FirstName: "Alice",
            LastName: "One",
            Username: username,
            EmailAddress: "a@example.com",
            TelephoneNumber: null,
            MobileNumber: null,
            EmailVerified: true,
            Gender: null,
            DateOfBirth: null,
            PackageId: Guid.Empty,
            Avatar: 17,
            UserRole: UserType.Customer,
            Skills: null,
            Hobbies: null)
        {
            Id = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

    private static GetUserByIdQuery.UserDto CreateGetUserByIdDto(Guid userId, string username)
        => new(
            Name: "Alice",
            FirstName: "Alice",
            LastName: "One",
            Username: username,
            EmailAddress: "a@example.com",
            TelephoneNumber: null,
            MobileNumber: null,
            EmailVerified: true,
            Gender: null,
            DateOfBirth: null,
            PackageId: Guid.Empty,
            Avatar: 17,
            UserRole: UserType.Customer,
            Skills: null,
            Hobbies: null)
        {
            Id = userId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

    private static SearchUserQuery.UserDto CreateSearchDto(string username)
        => new(
            Name: "Alice",
            FirstName: "Alice",
            LastName: "One",
            Username: username,
            EmailAddress: "a@example.com",
            TelephoneNumber: null,
            MobileNumber: null,
            EmailVerified: true,
            Gender: null,
            DateOfBirth: null,
            PackageId: Guid.Empty,
            Avatar: 17,
            UserRole: UserType.Customer,
            Skills: null,
            Hobbies: null)
        {
            Id = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

    private sealed class FakeDispatcher : IDispatcher
    {
        public List<object> DispatchedActions { get; } = new();
        public event EventHandler<ActionDispatchedEventArgs>? ActionDispatched;

        public void Dispatch(object action)
        {
            DispatchedActions.Add(action);
            ActionDispatched?.Invoke(this, new ActionDispatchedEventArgs(action));
        }
    }

    private sealed class FakeMediator : IMediator
    {
        public Func<object, Task<object?>>? SendHandler { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (SendHandler == null)
            {
                throw new InvalidOperationException("No send handler configured.");
            }

            return SendHandler(request).ContinueWith(t => (TResponse)t.Result!, cancellationToken);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            if (SendHandler == null)
            {
                throw new InvalidOperationException("No send handler configured.");
            }

            return SendHandler(request!);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            if (SendHandler == null)
            {
                throw new InvalidOperationException("No send handler configured.");
            }

            return SendHandler(request);
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class StaticUsersState : IState<UsersState>
    {
        public StaticUsersState(UsersState value)
        {
            Value = value;
        }

        public UsersState Value { get; }
        public event EventHandler? StateChanged;
    }

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public UserResponse? UpdatedUser { get; private set; }

        public Task<UserResponse?> GetCurrentUserAsync() => Task.FromResult<UserResponse?>(null);

        public Task UpdateCurrentUserAsync(UserResponse user)
        {
            UpdatedUser = user;
            return Task.CompletedTask;
        }
    }
}