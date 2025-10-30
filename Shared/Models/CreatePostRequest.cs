using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class CreatePostRequest
{
    public string? Title { get; set; }

    public string? Heading { get; set; }

    public string? Description { get; set; }

    public string? Content { get; set; }

    public string? Author { get; set; }

    public string? UserId { get; set; }

    public string? Category { get; set; }

    public string CreatedBy { get; set; } = "system";
}
