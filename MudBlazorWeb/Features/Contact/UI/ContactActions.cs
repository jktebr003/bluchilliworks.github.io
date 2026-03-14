using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Contact.UI;

public record SubmitContactMessageAction(CreateMessageRequest Request);
public record SubmitContactMessageSuccessAction(string Message);
public record SubmitContactMessageFailedAction(string ErrorMessage);
