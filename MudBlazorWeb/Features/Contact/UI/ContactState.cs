using Fluxor;

namespace MudBlazorWeb.Features.Contact.UI;

public record ContactState
{
    public bool IsSubmitting { get; init; }
    public string? SuccessMessage { get; init; }
    public string? ErrorMessage { get; init; }
}

public class ContactFeatureState : Feature<ContactState>
{
    public override string GetName() => nameof(ContactState);

    protected override ContactState GetInitialState() => new()
    {
        IsSubmitting = false
    };
}
