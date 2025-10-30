using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class QualificationResponse : BaseResponse
{
    public string? Title { get; set; }
    public string? Institution { get; set; }
    public int? Year { get; set; }
}