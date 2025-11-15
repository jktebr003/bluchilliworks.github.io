using Shared.Models;

namespace Web.Features.Authentication;

public record LoginAction(string Username, string Password);
public record LoginSuccessAction(bool IsSuccess);
public record LoginFailedAction(string ErrorMessage);
public record SetBusyAction(bool IsBusy);

public record RegisterAction(string FirstName, string LastName, string EmailAddress);
public record RegisterSuccessAction(bool IsSuccess);
public record RegisterFailedAction(string ErrorMessage);
