using System.Runtime.InteropServices;

namespace Multitool.NTInterop
{
    internal static class ExceptionThrower
    {
        public static COMException GetLastError(string message, uint funcRetCode)
        {
            return new COMException(message + ". (return code " + funcRetCode + ")", Marshal.GetLastWin32Error());
        }
    }
}
