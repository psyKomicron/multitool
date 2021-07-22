using System.Runtime.InteropServices;

namespace Multitool.NTInterop
{
    internal static class InteropHelper
    {
        public static COMException GetLastError(string message, uint funcRetCode)
        {
            return new COMException(message + ". (return code " + funcRetCode + ")", Marshal.GetLastWin32Error());
        }

        public static COMException GetLastError(string message)
        {
            return new COMException(message, Marshal.GetLastWin32Error());
        }
    }
}
