using System.ComponentModel.DataAnnotations;

namespace MudBlazorWeb.Shared.Enums;

/// <summary>
/// The Package Type
/// </summary>
public enum PackageType : short
{
    [Display(Name = "It's autumn")]
    None = 0,

    [Display(Name = "Monthly")]
    Monthly = 1,

    [Display(Name = "2-year Subscription")]
    TwoYearSubscription = 2
}
