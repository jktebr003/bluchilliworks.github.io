using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class BaseResponse
{
    public string? ID { get; set; }
    public string CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public string? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}
