namespace MudBlazorWeb.Shared.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Code { get; set; }
    public string Message { get; set; }
    public T Result { get; set; }
}

public record ApiResult<T>(T Value, bool Success, string? Code = null, string? Message = null);
