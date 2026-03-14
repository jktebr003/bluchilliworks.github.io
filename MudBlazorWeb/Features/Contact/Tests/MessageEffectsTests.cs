using Fluxor;
using MediatR;

using MudBlazorWeb.Features.Contact.Application;
using MudBlazorWeb.Features.Contact.UI;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

using Xunit;

namespace MudBlazorWeb.Features.Contact.Tests;

public class MessageEffectsTests
{
    [Fact]
    public async Task HandleSubmitContactMessage_ShouldDispatchSuccess_WhenMediatorReturnsSuccessfulResult()
    {
        var request = CreateRequest();
        object? capturedRequest = null;

        var mediator = new FakeMediator
        {
            SendHandler = req =>
            {
                capturedRequest = req;
                return Task.FromResult<object?>(new Result<string>(Guid.NewGuid().ToString(), true));
            }
        };

        var dispatcher = new FakeDispatcher();
        var effects = new ContactEffects(mediator);

        await effects.HandleSubmitContactMessage(new SubmitContactMessageAction(request), dispatcher);

        var cmd = Assert.IsType<CreateMessageCommand.Command>(capturedRequest);
        Assert.Same(request, cmd.Request);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var success = Assert.IsType<SubmitContactMessageSuccessAction>(action);
        Assert.Equal("Thank you for contacting us! We'll get back to you soon.", success.Message);
    }

    [Fact]
    public async Task HandleSubmitContactMessage_ShouldDispatchFailed_WhenMediatorReturnsUnsuccessfulResult()
    {
        var request = CreateRequest();

        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(
                new Result<string>(string.Empty, false, "CreateMessage.Validation", "Please provide email, subject, and message body."))
        };

        var dispatcher = new FakeDispatcher();
        var effects = new ContactEffects(mediator);

        await effects.HandleSubmitContactMessage(new SubmitContactMessageAction(request), dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<SubmitContactMessageFailedAction>(action);
        Assert.Equal("Failed to send message: Please provide email, subject, and message body.", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleSubmitContactMessage_ShouldDispatchFailed_WhenMediatorThrowsException()
    {
        var request = CreateRequest();

        var mediator = new FakeMediator
        {
            SendHandler = _ => throw new InvalidOperationException("Service unavailable")
        };

        var dispatcher = new FakeDispatcher();
        var effects = new ContactEffects(mediator);

        await effects.HandleSubmitContactMessage(new SubmitContactMessageAction(request), dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<SubmitContactMessageFailedAction>(action);
        Assert.Equal("An error occurred while sending your message: Service unavailable", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleSubmitContactMessage_ShouldPassRequestPayloadToMediator()
    {
        var request = new CreateMessageRequest
        {
            From = "Jane Doe",
            EmailAddress = "jane@example.com",
            Subject = "Specific subject",
            Body = "Specific body",
            SentOn = "2026-01-01T00:00:00Z",
            CreatedBy = "test"
        };

        CreateMessageRequest? capturedPayload = null;

        var mediator = new FakeMediator
        {
            SendHandler = req =>
            {
                capturedPayload = ((CreateMessageCommand.Command)req).Request;
                return Task.FromResult<object?>(new Result<string>("some-id", true));
            }
        };

        var dispatcher = new FakeDispatcher();
        var effects = new ContactEffects(mediator);

        await effects.HandleSubmitContactMessage(new SubmitContactMessageAction(request), dispatcher);

        Assert.NotNull(capturedPayload);
        Assert.Equal("jane@example.com", capturedPayload!.EmailAddress);
        Assert.Equal("Specific subject", capturedPayload.Subject);
        Assert.Equal("Jane Doe", capturedPayload.From);
    }

    [Fact]
    public async Task HandleSubmitContactMessage_ShouldIncludeExceptionMessage_InFailureDispatch()
    {
        var request = CreateRequest();

        var mediator = new FakeMediator
        {
            SendHandler = _ => throw new TimeoutException("Timeout connecting to database")
        };

        var dispatcher = new FakeDispatcher();
        var effects = new ContactEffects(mediator);

        await effects.HandleSubmitContactMessage(new SubmitContactMessageAction(request), dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<SubmitContactMessageFailedAction>(action);
        Assert.Contains("Timeout connecting to database", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleSubmitContactMessage_ShouldDispatchFailed_WhenResultHasNoMessage()
    {
        var request = CreateRequest();

        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(
                new Result<string>(string.Empty, false, "SomeCode", null))
        };

        var dispatcher = new FakeDispatcher();
        var effects = new ContactEffects(mediator);

        await effects.HandleSubmitContactMessage(new SubmitContactMessageAction(request), dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<SubmitContactMessageFailedAction>(action);
        Assert.Equal("Failed to send message: ", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleSubmitContactMessage_ShouldOnlyDispatchOnce_OnSuccess()
    {
        var request = CreateRequest();

        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(new Result<string>("id", true))
        };

        var dispatcher = new FakeDispatcher();
        var effects = new ContactEffects(mediator);

        await effects.HandleSubmitContactMessage(new SubmitContactMessageAction(request), dispatcher);

        Assert.Single(dispatcher.DispatchedActions);
    }

    [Fact]
    public async Task HandleSubmitContactMessage_ShouldOnlyDispatchOnce_OnFailure()
    {
        var request = CreateRequest();

        var mediator = new FakeMediator
        {
            SendHandler = _ => throw new Exception("boom")
        };

        var dispatcher = new FakeDispatcher();
        var effects = new ContactEffects(mediator);

        await effects.HandleSubmitContactMessage(new SubmitContactMessageAction(request), dispatcher);

        Assert.Single(dispatcher.DispatchedActions);
    }

    private static CreateMessageRequest CreateRequest(
        string email = "test@example.com",
        string subject = "Hello",
        string body = "Test message")
    {
        return new CreateMessageRequest
        {
            From = "Test User",
            EmailAddress = email,
            Subject = subject,
            Body = body,
            SentOn = DateTime.UtcNow.ToString("o"),
            CreatedBy = "test"
        };
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
                throw new InvalidOperationException("No send handler configured.");

            return SendHandler(request).ContinueWith(t => (TResponse)t.Result!, cancellationToken);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            if (SendHandler == null)
                throw new InvalidOperationException("No send handler configured.");

            return SendHandler(request!);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            if (SendHandler == null)
                throw new InvalidOperationException("No send handler configured.");

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
