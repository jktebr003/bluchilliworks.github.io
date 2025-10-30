using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class PagedApiResponse<T>
{
    public bool Success { get; set; }
    public string Code { get; set; }
    public string Message { get; set; }
    public T Result { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalItems { get; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
}

public record PagedApiResult<T>(T Value, bool Success, int TotalPages, int TotalItems, int? PageNumber = null, int? PageSize = null, string? Code = null, string? Message = null);

