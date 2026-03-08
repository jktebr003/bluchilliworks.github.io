namespace MudBlazorWeb.Shared.Models;

public class CommentResponse : BaseResponse
{
    public string? PostId { get; set; }

    public string? Content { get; set; }

    public string? CommentedOn { get; set; }

    public string? Author { get; set; }
}
