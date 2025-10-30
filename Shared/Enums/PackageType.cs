using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums;

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

//public class Foo
//{
//    public PackageType packageType = PackageType.TwoYearSubscription;

//    public void DisplayName()
//    {
//        var packageTypeDisplayName = PackageType.GetAttribute<DisplayAttribute>();
//        Console.WriteLine("Which package type is it?");
//        Console.WriteLine(packageTypeDisplayName.Name);
//    }
//}
