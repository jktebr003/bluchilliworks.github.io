using MudBlazorWeb.Features.Audits.Application;

namespace MudBlazorWeb.Features.Audits.UI;

public class AuditsAction
{
    public record LoadAuditsAction;
    public record LoadAuditsSuccessAction(List<GetAuditsQuery.AuditDto> Audits);
    public record LoadAuditsFailedAction(string ErrorMessage);
}
