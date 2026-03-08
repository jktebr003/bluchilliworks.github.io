namespace MudBlazorWeb.Shared.Services;

public class BusyDialogService
{
    private bool _isBusy;
    private string _message = "Loading...";

    public bool IsBusy => _isBusy;
    public string Message => _message;

    public event Action? OnChange;

    /// <summary>
    /// Shows the busy dialog with an optional message
    /// </summary>
    /// <param name="message">The message to display (defaults to "Loading...")</param>
    public void Show(string message = "Loading...")
    {
        _message = message;
        _isBusy = true;
        NotifyStateChanged();
    }

    /// <summary>
    /// Hides the busy dialog
    /// </summary>
    public void Hide()
    {
        _isBusy = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
