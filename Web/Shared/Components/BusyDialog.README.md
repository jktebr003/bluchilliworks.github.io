# BusyDialog Component

A reusable busy dialog component that displays a loading spinner with a customizable message, similar to Radzen's confirm dialog.

## Features

- Non-dismissible dialog with a loading spinner
- Customizable message
- Centered on screen
- Uses MudBlazor components for consistent styling
- Service-based API for easy use from anywhere in the application

## Usage

### Basic Usage

Inject the `BusyDialogService` into your component or page:

```razor
@inject BusyDialogService BusyDialogService
```

Show the busy dialog:

```csharp
// Show with default "Loading..." message
BusyDialogService.Show();

// Show with custom message
BusyDialogService.Show("Processing your request...");
```

Hide the busy dialog:

```csharp
BusyDialogService.Hide();
```

### Example in a Blazor Component

```razor
@page "/example"
@inject BusyDialogService BusyDialogService

<MudButton OnClick="PerformLongOperation" Color="Color.Primary">
    Start Long Operation
</MudButton>

@code {
    private async Task PerformLongOperation()
    {
        // Show busy dialog
        BusyDialogService.Show("Processing data...");
        
        try
        {
            // Simulate a long-running operation
            await Task.Delay(3000);
            
            // Your actual logic here
            await SomeApiCall();
        }
        finally
        {
            // Always hide the dialog, even if an error occurs
            BusyDialogService.Hide();
        }
    }
    
    private async Task SomeApiCall()
    {
        // Your API call logic
    }
}
```

### Example with Try-Catch

```csharp
private async Task SaveData()
{
    BusyDialogService.Show("Saving changes...");
    
    try
    {
        await apiClient.SaveAsync(data);
        // Success handling
    }
    catch (Exception ex)
    {
        // Error handling
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        BusyDialogService.Hide();
    }
}
```

### Example with Multiple Steps

```csharp
private async Task ProcessMultipleSteps()
{
    BusyDialogService.Show("Step 1: Loading data...");
    await LoadData();
    
    BusyDialogService.Show("Step 2: Processing...");
    await ProcessData();
    
    BusyDialogService.Show("Step 3: Saving results...");
    await SaveResults();
    
    BusyDialogService.Hide();
}
```

## Implementation Details

### Service

The `BusyDialogService` is registered as a singleton in `Program.cs` and provides:

- `Show(string message = "Loading...")` - Shows the dialog with an optional message
- `Hide()` - Hides the dialog
- `IsBusy` - Property to check if the dialog is currently shown
- `Message` - Property to get the current message

### Component

The `BusyDialog` component:

- Uses MudBlazor's `MudDialog` component
- Displays a `MudProgressCircular` spinner
- Shows the current message below the spinner
- Disables backdrop click (cannot be dismissed by clicking outside)
- Has no close button (must be dismissed programmatically)

## Styling

The component uses MudBlazor's default styling and can be customized by:

1. Modifying the `DialogOptions` in `BusyDialog.razor`
2. Adjusting the spinner size, color, or position
3. Customizing the text typography

## Notes

- Always ensure `Hide()` is called, preferably in a `finally` block
- The service is thread-safe and can be called from multiple components
- The dialog is non-modal but prevents user interaction with backdrop clicks disabled
