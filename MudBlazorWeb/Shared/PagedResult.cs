namespace MudBlazorWeb.Shared;

public class PagedResponse<T>
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

public record PagedResult<T>(T Value, bool Success, int TotalPages, int TotalItems, int? PageNumber = null, int? PageSize = null, string? Code = null, string? Message = null);


