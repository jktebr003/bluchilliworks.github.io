using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class MessageResponse : BaseResponse
{
    public string? From { get; set; }

    public string? EmailAddress { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public string? SentOn { get; set; }
}
