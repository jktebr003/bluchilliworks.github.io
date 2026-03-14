using Fluxor;

namespace MudBlazorWeb.Features.Contact.UI;

public static class ContactReducers
{
    [ReducerMethod(typeof(SubmitContactMessageAction))]
    public static ContactState ReduceSubmitContactMessageAction(ContactState state)
    {
        return state with
        {
            IsSubmitting = true,
            SuccessMessage = null,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static ContactState ReduceSubmitContactMessageSuccessAction(ContactState state, SubmitContactMessageSuccessAction action)
    {
        return state with
        {
            IsSubmitting = false,
            SuccessMessage = action.Message,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static ContactState ReduceSubmitContactMessageFailedAction(ContactState state, SubmitContactMessageFailedAction action)
    {
        return state with
        {
            IsSubmitting = false,
            ErrorMessage = action.ErrorMessage,
            SuccessMessage = null
        };
    }
}
