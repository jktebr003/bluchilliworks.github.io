using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class CommentResponse : BaseResponse
{
    public string? PostId { get; set; }

    public string? Content { get; set; }

    public string? CommentedOn { get; set; }

    public string? Author { get; set; }
}
