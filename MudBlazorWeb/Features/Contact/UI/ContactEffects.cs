using Fluxor;
using MediatR;

using MudBlazorWeb.Features.Contact.Application;

namespace MudBlazorWeb.Features.Contact.UI;

public class ContactEffects
{
    private readonly IMediator _mediator;

    public ContactEffects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [EffectMethod]
    public async Task HandleSubmitContactMessage(SubmitContactMessageAction action, IDispatcher dispatcher)
    {
        try
        {
            await Task.Yield();

            var response = await _mediator.Send(new global::MudBlazorWeb.Features.Contact.Application.CreateMessageCommand.Command(action.Request));

            if (response.Success)
            {
                dispatcher.Dispatch(new SubmitContactMessageSuccessAction("Thank you for contacting us! We'll get back to you soon."));
            }
            else
            {
                dispatcher.Dispatch(new SubmitContactMessageFailedAction($"Failed to send message: {response.Message}"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SubmitContactMessageFailedAction($"An error occurred while sending your message: {ex.Message}"));
        }
    }
}
