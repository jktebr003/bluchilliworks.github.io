using Fluxor;

using static MudBlazorWeb.Features.Audits.UI.AuditsAction;

namespace MudBlazorWeb.Features.Audits.UI;

public class AuditsReducers
{
    [ReducerMethod(typeof(LoadAuditsAction))]
    public static AuditsState ReduceLoadAudits(AuditsState state) =>
        state with { IsLoading = true, ErrorMessage = null };

    [ReducerMethod]
    public static AuditsState ReduceLoadAuditsSuccess(AuditsState state, LoadAuditsSuccessAction action) =>
        state with { IsLoading = false, ErrorMessage = null, Audits = action.Audits };

    [ReducerMethod]
    public static AuditsState ReduceLoadAuditsFailed(AuditsState state, LoadAuditsFailedAction action) =>
        state with { IsLoading = false, ErrorMessage = action.ErrorMessage };
}
