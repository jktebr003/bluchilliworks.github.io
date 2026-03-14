using MudBlazorWeb.Features.Audits.Application;
using MudBlazorWeb.Features.Audits.UI;

using static MudBlazorWeb.Features.Audits.UI.AuditsAction;

using Xunit;

namespace MudBlazorWeb.Features.Audits.Tests;

public class AuditReducerTests
{
    [Fact]
    public void ReduceLoadAudits_ShouldSetLoadingTrue_AndClearError()
    {
        var audits = CreateAudits();
        var state = new AuditsState
        {
            IsLoading = false,
            ErrorMessage = "old error",
            Audits = audits
        };

        var result = AuditsReducers.ReduceLoadAudits(state);

        Assert.NotSame(state, result);
        Assert.True(result.IsLoading);
        Assert.Null(result.ErrorMessage);
        Assert.Same(audits, result.Audits);
    }

    [Fact]
    public void ReduceLoadAuditsSuccess_ShouldPopulateAudits_AndClearError()
    {
        var audits = CreateAudits();
        var state = new AuditsState
        {
            IsLoading = true,
            ErrorMessage = "old error"
        };

        var action = new LoadAuditsSuccessAction(audits);

        var result = AuditsReducers.ReduceLoadAuditsSuccess(state, action);

        Assert.False(result.IsLoading);
        Assert.Null(result.ErrorMessage);
        Assert.Same(audits, result.Audits);
    }

    [Fact]
    public void ReduceLoadAuditsSuccess_ShouldHandleEmptyList()
    {
        var state = new AuditsState
        {
            IsLoading = true,
            ErrorMessage = "old error",
            Audits = CreateAudits()
        };

        var action = new LoadAuditsSuccessAction(new List<GetAuditsQuery.AuditDto>());

        var result = AuditsReducers.ReduceLoadAuditsSuccess(state, action);

        Assert.False(result.IsLoading);
        Assert.Null(result.ErrorMessage);
        Assert.Empty(result.Audits);
    }

    [Fact]
    public void ReduceLoadAuditsFailed_ShouldSetError_AndStopLoading()
    {
        var audits = CreateAudits();
        var state = new AuditsState
        {
            IsLoading = true,
            Audits = audits
        };

        var action = new LoadAuditsFailedAction("Network timeout");

        var result = AuditsReducers.ReduceLoadAuditsFailed(state, action);

        Assert.False(result.IsLoading);
        Assert.Equal("Network timeout", result.ErrorMessage);
        Assert.Same(audits, result.Audits);
    }

    private static List<GetAuditsQuery.AuditDto> CreateAudits()
    {
        return new List<GetAuditsQuery.AuditDto>
        {
            new(Guid.NewGuid(), "Create", "alice@example.com", "Packages", "{\"Id\":\"1\"}", null, null, "[\"Name\"]", "system", new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
            new(Guid.NewGuid(), "Update", "bob@example.com", "Messages", "{\"Id\":\"2\"}", null, null, "[\"Status\"]", "system", new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc))
        };
    }
}