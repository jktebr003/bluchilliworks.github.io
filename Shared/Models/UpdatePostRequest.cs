using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class UpdatePostRequest
{
    public string Id { get; set; }

    public string? Title { get; set; }

    public string? Heading { get; set; }

    public string? Description { get; set; }

    public string? Content { get; set; }

    public string? Author { get; set; }

    public string? UserId { get; set; }

    public string? Category { get; set; }

    public int? TotalViews { get; set; }

    public List<string> Likes { get; set; } = [];

    //public List<CommentResponse> Comments { get; set; } = [];

    public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

    public string ModifiedBy { get; set; } = "system";
}
