using System;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Users.Domain;

public interface IUserRepository
{
    public Task<List<User>> FilterUsersByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default);
    public Task<User?> GetUserByEmailAddressAsync(string? emailAddress, CancellationToken cancellationToken = default);
    public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    public Task<User> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);    
    public Task SaveUserAsync(User user, CancellationToken cancellationToken = default);
    public Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    public Task<List<User>> SearchUsersAsync(
        string? search = null,
        UserType? role = null,
        string? gender = null,
        string? package = null,
        bool? emailVerified = null,
        DateTime? dobFrom = null,
        DateTime? dobTo = null,
        CancellationToken cancellationToken = default
    );
}
