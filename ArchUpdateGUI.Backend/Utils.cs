using System.Runtime.InteropServices;
using System.Security;

namespace ArchUpdateGUI.Backend;

public static class Utils
{
    public static string? SecureToString(this SecureString secure) =>
        Marshal.PtrToStringUni(Marshal.SecureStringToBSTR(secure));
}