using Shared.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class PackageResponse : BaseResponse
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Code { get; set; }

    public string? Price { get; set; }

    public PackageType Type { get; set; }

    public bool ShowPackage { get; set; }

    // Implement this for the Pizza to display correctly in MudSelect
    public override string ToString() => Name;
}
