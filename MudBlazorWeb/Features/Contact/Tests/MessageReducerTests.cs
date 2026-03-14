using MudBlazorWeb.Features.Contact.UI;

using Xunit;

namespace MudBlazorWeb.Features.Contact.Tests;

public class MessageReducerTests
{
    // ======================================================================
    // ReduceSubmitContactMessageAction  (action: SubmitContactMessageAction)
    // ======================================================================

    [Fact]
    public void ReduceSubmitContactMessageAction_ShouldSetIsSubmittingTrue_AndClearBothMessages()
    {
        var state = new ContactState
        {
            IsSubmitting = false,
            SuccessMessage = "previous success",
            ErrorMessage = "previous error"
        };

        var result = ContactReducers.ReduceSubmitContactMessageAction(state);

        Assert.NotSame(state, result);
        Assert.True(result.IsSubmitting);
        Assert.Null(result.SuccessMessage);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ReduceSubmitContactMessageAction_ShouldSetIsSubmittingTrue_WhenStateIsAlreadyClean()
    {
        var state = new ContactState
        {
            IsSubmitting = false,
            SuccessMessage = null,
            ErrorMessage = null
        };

        var result = ContactReducers.ReduceSubmitContactMessageAction(state);

        Assert.True(result.IsSubmitting);
        Assert.Null(result.SuccessMessage);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ReduceSubmitContactMessageAction_ShouldReturnNewStateInstance_EachCall()
    {
        var state = new ContactState();

        var result1 = ContactReducers.ReduceSubmitContactMessageAction(state);
        var result2 = ContactReducers.ReduceSubmitContactMessageAction(state);

        Assert.NotSame(result1, result2);
    }

    [Fact]
    public void ReduceSubmitContactMessageAction_ShouldOverrideIsSubmitting_WhenAlreadyTrue()
    {
        var state = new ContactState
        {
            IsSubmitting = true,
            SuccessMessage = "old success",
            ErrorMessage = null
        };

        var result = ContactReducers.ReduceSubmitContactMessageAction(state);

        Assert.True(result.IsSubmitting);
        Assert.Null(result.SuccessMessage);
        Assert.Null(result.ErrorMessage);
    }

    // ======================================================================
    // ReduceSubmitContactMessageSuccessAction
    // ======================================================================

    [Fact]
    public void ReduceSubmitContactMessageSuccessAction_ShouldSetSuccessMessage_AndClearErrorAndIsSubmitting()
    {
        var state = new ContactState
        {
            IsSubmitting = true,
            SuccessMessage = null,
            ErrorMessage = "old error"
        };

        var action = new SubmitContactMessageSuccessAction("Thank you for contacting us! We'll get back to you soon.");

        var result = ContactReducers.ReduceSubmitContactMessageSuccessAction(state, action);

        Assert.NotSame(state, result);
        Assert.False(result.IsSubmitting);
        Assert.Equal("Thank you for contacting us! We'll get back to you soon.", result.SuccessMessage);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ReduceSubmitContactMessageSuccessAction_ShouldStoreExactMessageFromAction()
    {
        var state = new ContactState { IsSubmitting = true };
        const string customMessage = "Custom success message for the user";

        var action = new SubmitContactMessageSuccessAction(customMessage);

        var result = ContactReducers.ReduceSubmitContactMessageSuccessAction(state, action);

        Assert.Equal(customMessage, result.SuccessMessage);
    }

    [Fact]
    public void ReduceSubmitContactMessageSuccessAction_ShouldClearPreviousSuccessMessage_WhenNewSuccessArrives()
    {
        var state = new ContactState
        {
            IsSubmitting = true,
            SuccessMessage = "first success"
        };

        var action = new SubmitContactMessageSuccessAction("second success");

        var result = ContactReducers.ReduceSubmitContactMessageSuccessAction(state, action);

        Assert.Equal("second success", result.SuccessMessage);
    }

    [Fact]
    public void ReduceSubmitContactMessageSuccessAction_ShouldReturnNewStateInstance()
    {
        var state = new ContactState { IsSubmitting = true };
        var action = new SubmitContactMessageSuccessAction("ok");

        var result1 = ContactReducers.ReduceSubmitContactMessageSuccessAction(state, action);
        var result2 = ContactReducers.ReduceSubmitContactMessageSuccessAction(state, action);

        Assert.NotSame(result1, result2);
    }

    // ======================================================================
    // ReduceSubmitContactMessageFailedAction
    // ======================================================================

    [Fact]
    public void ReduceSubmitContactMessageFailedAction_ShouldSetErrorMessage_AndClearSuccessAndIsSubmitting()
    {
        var state = new ContactState
        {
            IsSubmitting = true,
            SuccessMessage = "old success",
            ErrorMessage = null
        };

        var action = new SubmitContactMessageFailedAction("Network error occurred");

        var result = ContactReducers.ReduceSubmitContactMessageFailedAction(state, action);

        Assert.NotSame(state, result);
        Assert.False(result.IsSubmitting);
        Assert.Equal("Network error occurred", result.ErrorMessage);
        Assert.Null(result.SuccessMessage);
    }

    [Fact]
    public void ReduceSubmitContactMessageFailedAction_ShouldStoreExactErrorMessageFromAction()
    {
        var state = new ContactState { IsSubmitting = true };
        const string errorMessage = "Failed to send message: Please provide email, subject, and message body.";

        var action = new SubmitContactMessageFailedAction(errorMessage);

        var result = ContactReducers.ReduceSubmitContactMessageFailedAction(state, action);

        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void ReduceSubmitContactMessageFailedAction_ShouldOverridePreviousError_WhenNewFailureArrives()
    {
        var state = new ContactState
        {
            IsSubmitting = true,
            ErrorMessage = "first error"
        };

        var action = new SubmitContactMessageFailedAction("second error");

        var result = ContactReducers.ReduceSubmitContactMessageFailedAction(state, action);

        Assert.Equal("second error", result.ErrorMessage);
    }

    [Fact]
    public void ReduceSubmitContactMessageFailedAction_ShouldReturnNewStateInstance()
    {
        var state = new ContactState { IsSubmitting = true };
        var action = new SubmitContactMessageFailedAction("error");

        var result1 = ContactReducers.ReduceSubmitContactMessageFailedAction(state, action);
        var result2 = ContactReducers.ReduceSubmitContactMessageFailedAction(state, action);

        Assert.NotSame(result1, result2);
    }

    // ======================================================================
    // State-transition sequence scenarios
    // ======================================================================

    [Fact]
    public void FullSuccessSequence_ShouldProduceExpectedStateTransitions()
    {
        // Arrange – initial state (as set by ContactFeatureState.GetInitialState)
        var initial = new ContactState { IsSubmitting = false };

        // Act 1 – user submits the form
        var submitting = ContactReducers.ReduceSubmitContactMessageAction(initial);

        // Act 2 – mediator succeeds
        var successAction = new SubmitContactMessageSuccessAction("Thank you!");
        var succeeded = ContactReducers.ReduceSubmitContactMessageSuccessAction(submitting, successAction);

        // Assert intermediate
        Assert.True(submitting.IsSubmitting);
        Assert.Null(submitting.SuccessMessage);
        Assert.Null(submitting.ErrorMessage);

        // Assert final
        Assert.False(succeeded.IsSubmitting);
        Assert.Equal("Thank you!", succeeded.SuccessMessage);
        Assert.Null(succeeded.ErrorMessage);
    }

    [Fact]
    public void FullFailureSequence_ShouldProduceExpectedStateTransitions()
    {
        // Arrange – initial state
        var initial = new ContactState { IsSubmitting = false };

        // Act 1 – user submits form
        var submitting = ContactReducers.ReduceSubmitContactMessageAction(initial);

        // Act 2 – mediator fails
        var failedAction = new SubmitContactMessageFailedAction("An error occurred while sending your message: Connection refused");
        var failed = ContactReducers.ReduceSubmitContactMessageFailedAction(submitting, failedAction);

        // Assert intermediate
        Assert.True(submitting.IsSubmitting);

        // Assert final
        Assert.False(failed.IsSubmitting);
        Assert.Equal("An error occurred while sending your message: Connection refused", failed.ErrorMessage);
        Assert.Null(failed.SuccessMessage);
    }

    [Fact]
    public void RetryAfterFailure_ShouldClearErrorOnResubmit()
    {
        // Arrange – state after a prior failure
        var failedState = new ContactState
        {
            IsSubmitting = false,
            ErrorMessage = "Previous error"
        };

        // Act – user resubmits
        var resubmitting = ContactReducers.ReduceSubmitContactMessageAction(failedState);

        Assert.True(resubmitting.IsSubmitting);
        Assert.Null(resubmitting.ErrorMessage);
        Assert.Null(resubmitting.SuccessMessage);
    }
}
