using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ArchUpdateGUI.Models;

public static class Utils
{
    public static string? SecureToString(this SecureString secure)
    {
        IntPtr point = Marshal.SecureStringToBSTR(secure);
        return Marshal.PtrToStringUni(point);
    }
}