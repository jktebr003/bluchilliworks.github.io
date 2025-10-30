using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Code { get; set; }
    public string Message { get; set; }
    public T Result { get; set; }
}

public record ApiResult<T>(T Value, bool Success, string? Code = null, string? Message = null);
