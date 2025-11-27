using Shared.Models;

namespace Web.Features.Authentication;

public record LoginAction(string Username, string Password);
public record LoginSuccessAction(bool IsSuccess);
public record LoginFailedAction(string ErrorMessage);
public record SetBusyAction(bool IsBusy);

public record RegisterAction(string FirstName, string LastName, string EmailAddress);
public record RegisterSuccessAction(bool IsSuccess);
public record RegisterFailedAction(string ErrorMessage);

public record SetPasswordAction(string EmailAddress, string VerificationToken, string Password);
public record SetPasswordSuccessAction(bool IsSuccess);
public record SetPasswordFailedAction(string ErrorMessage);

public record ResendVerificationAction(string EmailAddress);
public record ResendVerificationSuccessAction(bool IsSuccess);
public record ResendVerificationFailedAction(string ErrorMessage);

public record AuthenticationFailedAction(string ErrorMessage);
