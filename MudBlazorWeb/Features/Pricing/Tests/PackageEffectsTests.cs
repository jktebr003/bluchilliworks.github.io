using Fluxor;
using MediatR;

using MudBlazorWeb.Features.Pricing.Application;
using MudBlazorWeb.Features.Pricing.UI;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;

using static MudBlazorWeb.Features.Pricing.UI.PricingAction;

using Xunit;

namespace MudBlazorWeb.Features.Pricing.Tests;

public class PackageEffectsTests
{
    [Fact]
    public async Task HandleLoadPackages_ShouldDispatchSuccess_WhenMediatorReturnsSuccessfulResult()
    {
        var packages = new List<GetPackagesQuery.PackageDto>
        {
            CreatePackage("Starter", PackageType.Monthly),
            CreatePackage("Pro", PackageType.TwoYearSubscription)
        };

        object? capturedRequest = null;
        var mediator = new FakeMediator
        {
            SendHandler = request =>
            {
                capturedRequest = request;
                var result = new PagedResult<List<GetPackagesQuery.PackageDto>>(packages, true, 1, 2, 1, 10);
                return Task.FromResult<object?>(result);
            }
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PricingEffects(mediator);

        await effects.HandleLoadPackages(dispatcher);

        var query = Assert.IsType<GetPackagesQuery.Query>(capturedRequest);
        Assert.Null(query.PageSize);
        Assert.Null(query.PageNumber);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var success = Assert.IsType<LoadPackagesSuccessAction>(action);
        Assert.Equal(2, success.Packages.Count);
    }

    [Fact]
    public async Task HandleLoadPackages_ShouldDispatchFailed_WhenMediatorReturnsUnsuccessfulResultWithMessage()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(
                new PagedResult<List<GetPackagesQuery.PackageDto>>(
                    Value: new List<GetPackagesQuery.PackageDto>(),
                    Success: false,
                    TotalPages: 0,
                    TotalItems: 0,
                    Message: "Backend returned failure"))
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PricingEffects(mediator);

        await effects.HandleLoadPackages(dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadPackagesFailedAction>(action);
        Assert.Equal("Backend returned failure", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleLoadPackages_ShouldDispatchFailedWithFallback_WhenMediatorReturnsUnsuccessfulResultWithoutMessage()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(
                new PagedResult<List<GetPackagesQuery.PackageDto>>(
                    Value: new List<GetPackagesQuery.PackageDto>(),
                    Success: false,
                    TotalPages: 0,
                    TotalItems: 0,
                    Message: null))
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PricingEffects(mediator);

        await effects.HandleLoadPackages(dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadPackagesFailedAction>(action);
        Assert.Equal("Failed to load packages", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleLoadPackages_ShouldDispatchFailed_WhenMediatorThrowsException()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => throw new InvalidOperationException("Connection refused")
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PricingEffects(mediator);

        await effects.HandleLoadPackages(dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadPackagesFailedAction>(action);
        Assert.Equal("Error loading packages: Connection refused", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleLoadPackages_ShouldDispatchFailedWithFallback_WhenMediatorReturnsNull()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(null)
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PricingEffects(mediator);

        await effects.HandleLoadPackages(dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadPackagesFailedAction>(action);
        Assert.Equal("Failed to load packages", failed.ErrorMessage);
    }

    private static GetPackagesQuery.PackageDto CreatePackage(string name, PackageType type)
    {
        return new GetPackagesQuery.PackageDto(name, $"{name} description", name.ToUpperInvariant(), "10", type, true)
        {
            Id = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
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
