using MudBlazorWeb.Shared.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudBlazorWeb.Shared.Models;

public class UpdateUserSessionRequest
{
    public string? UserId { get; set; }
    public string? SessionToken { get; set; }
    public DateTimeOffset? LastAccessedOn { get; set; }
    public bool IsExpired { get; set; }
    public DateTime ModifiedOn { get; set; } = DateTimeExtension.GetSouthAfricanTime();
    public string ModifiedBy { get; set; } = "system";
}
