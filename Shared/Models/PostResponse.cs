using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class PostResponse : BaseResponse
{
    public string? Title { get; set; }

    public string? Heading { get; set; }

    public string? Description { get; set; }

    public string? Content { get; set; }

    public string? Author { get; set; }

    public string? UserId { get; set; }

    public string? Category { get; set; }

    public int? TotalViews { get; set; }

    public int? TotalLikes { get; set; }

    public int? TotalComments { get; set; }

    //public List<string> Likes { get; set; } = [];

    //public Image? CoverImage { get; set; }

    public string? PostedOn { get; set; }

    //public DateTime? DateEdited { get; set; }

    //public List<CommentResponse> Comments { get; set; } = [];

    //public List<Image> Images { get; set; } = new List<Image>();
}
