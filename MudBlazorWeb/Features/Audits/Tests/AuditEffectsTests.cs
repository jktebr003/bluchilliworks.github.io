using Fluxor;
using MediatR;

using MudBlazorWeb.Features.Audits.Application;
using MudBlazorWeb.Features.Audits.UI;
using MudBlazorWeb.Shared;

using static MudBlazorWeb.Features.Audits.UI.AuditsAction;

using Xunit;

namespace MudBlazorWeb.Features.Audits.Tests;

public class AuditEffectsTests
{
    [Fact]
    public async Task HandleLoadAudits_ShouldDispatchSuccess_WhenMediatorReturnsSuccessfulResult()
    {
        var audits = new List<GetAuditsQuery.AuditDto>
        {
            CreateAudit("Create"),
            CreateAudit("Update")
        };

        object? capturedRequest = null;
        var mediator = new FakeMediator
        {
            SendHandler = request =>
            {
                capturedRequest = request;
                var result = new PagedResult<List<GetAuditsQuery.AuditDto>>(audits, true, 1, 2, 1, 10);
                return Task.FromResult<object?>(result);
            }
        };

        var dispatcher = new FakeDispatcher();
        var effects = new AuditsEffects(mediator);

        await effects.HandleLoadAudits(dispatcher);

        var query = Assert.IsType<GetAuditsQuery.Query>(capturedRequest);
        Assert.Null(query.PageSize);
        Assert.Null(query.PageNumber);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var success = Assert.IsType<LoadAuditsSuccessAction>(action);
        Assert.Equal(2, success.Audits.Count);
    }

    [Fact]
    public async Task HandleLoadAudits_ShouldDispatchFailed_WhenMediatorReturnsUnsuccessfulResultWithMessage()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(
                new PagedResult<List<GetAuditsQuery.AuditDto>>(
                    Value: new List<GetAuditsQuery.AuditDto>(),
                    Success: false,
                    TotalPages: 0,
                    TotalItems: 0,
                    Message: "Backend returned failure"))
        };

        var dispatcher = new FakeDispatcher();
        var effects = new AuditsEffects(mediator);

        await effects.HandleLoadAudits(dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadAuditsFailedAction>(action);
        Assert.Equal("Backend returned failure", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleLoadAudits_ShouldDispatchFailedWithFallback_WhenMediatorReturnsUnsuccessfulResultWithoutMessage()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(
                new PagedResult<List<GetAuditsQuery.AuditDto>>(
                    Value: new List<GetAuditsQuery.AuditDto>(),
                    Success: false,
                    TotalPages: 0,
                    TotalItems: 0,
                    Message: null))
        };

        var dispatcher = new FakeDispatcher();
        var effects = new AuditsEffects(mediator);

        await effects.HandleLoadAudits(dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadAuditsFailedAction>(action);
        Assert.Equal("Failed to load audits", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleLoadAudits_ShouldDispatchFailed_WhenMediatorThrowsException()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => throw new InvalidOperationException("Connection refused")
        };

        var dispatcher = new FakeDispatcher();
        var effects = new AuditsEffects(mediator);

        await effects.HandleLoadAudits(dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadAuditsFailedAction>(action);
        Assert.Equal("Error loading audits: Connection refused", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleLoadAudits_ShouldDispatchFailedWithFallback_WhenMediatorReturnsNull()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(null)
        };

        var dispatcher = new FakeDispatcher();
        var effects = new AuditsEffects(mediator);

        await effects.HandleLoadAudits(dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadAuditsFailedAction>(action);
        Assert.Equal("Failed to load audits", failed.ErrorMessage);
    }

    private static GetAuditsQuery.AuditDto CreateAudit(string auditType)
    {
        return new GetAuditsQuery.AuditDto(
            Guid.NewGuid(),
            auditType,
            "test@example.com",
            "Messages",
            "{\"Id\":\"1\"}",
            null,
            null,
            "[\"Status\"]",
            "test",
            DateTime.UtcNow);
    }

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
            => throw new NotSupportedException("Streaming is not used in these tests.");

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Streaming is not used in these tests.");
    }
}