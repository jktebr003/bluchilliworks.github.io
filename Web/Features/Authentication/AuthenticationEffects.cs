using Fluxor;
using Microsoft.AspNetCore.Components;

namespace Web.Features.Authentication;

public class AuthenticationEffects
{
    private readonly IAuthenticationService _authService;
    private readonly NavigationManager _navigationManager;

    public AuthenticationEffects(IAuthenticationService authService, NavigationManager navigationManager)
    {
        _authService = authService;
        _navigationManager = navigationManager;
    }

    [EffectMethod]
    public async Task HandleLoginAction(LoginAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetBusyAction(true));
        var result = await _authService.LoginAsync(action.Username, action.Password);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoginSuccessAction(result.Succeeded));
            _navigationManager.NavigateTo("/", true); // <-- Redirect after success
        }
        else
        {
            dispatcher.Dispatch(new LoginFailedAction(result.ErrorMessage ?? "Login failed"));
        }
    }
}
