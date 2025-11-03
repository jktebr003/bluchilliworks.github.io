using Shared.Models;

using Solutaris.InfoWARE.ProtectedBrowserStorage.Services;

namespace Web.Shared.Helpers;

public class LocalStorageHelper
{
    private IIWLocalStorageService _localStorage { get; set; }

    public LocalStorageHelper(IIWLocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<UserResponse> GetCurrentUserAsync()
    {
        return await _localStorage.GetItemAsync<UserResponse>("currentUser");
    }

    public async Task<string> GetItemAsync(string key)
    {
        return await _localStorage.GetItemAsync<string>(key);
    }

    public async Task SetCurrentUserAsync(UserResponse user)
    {
        await _localStorage.SetItemAsync("currentUser", user);
    }

    public async Task SetItemAsync(string key, string value)
    {
        await _localStorage.SetItemAsync(key, value);
    }

    public async Task RemoveCurrentUserAsync()
    {
        await _localStorage.RemoveItemAsync("currentUser");
    }

    public async Task RemoveItemAsync(string key)
    {
        await _localStorage.RemoveItemAsync(key);
    }

    public async Task ClearAllItems()
    {
        await _localStorage.RemoveAllItemsAsync();
    }
}
