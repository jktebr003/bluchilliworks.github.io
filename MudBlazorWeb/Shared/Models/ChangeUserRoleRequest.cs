using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Shared.Models;

public class ChangeUserRoleRequest
{
    public UserType NewRole { get; set; }
    public string? RequestingUserId { get; set; }
}
