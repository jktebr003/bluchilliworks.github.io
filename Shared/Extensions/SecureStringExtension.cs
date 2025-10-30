using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Extensions;

/// <summary>
/// The Secure String Extension Static Class
/// </summary>
public static class SecureStringExtension
{
    /// <summary>
    /// Converts a <see cref="String"/> into <see cref="SecureString"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static SecureString CreateSecure(this string value)
    {
        SecureString ss = new SecureString();

        if (String.IsNullOrEmpty(value)) return ss;

        foreach (char c in value)
        {
            ss.AppendChar(c);
        }

        return ss;
    }

    /// <summary>
    /// Converts a <see cref="SecureString"/> into <see cref="String"/>
    /// </summary>
    /// <param name="ss"></param>
    /// <returns></returns>
    public static string ToPlain(this SecureString ss)
    {
        var returnValue = IntPtr.Zero;
        try
        {
            returnValue = Marshal.SecureStringToGlobalAllocUnicode(ss);
            return Marshal.PtrToStringUni(returnValue);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(returnValue);
        }
    }
}
