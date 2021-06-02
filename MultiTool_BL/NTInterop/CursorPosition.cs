using System.Runtime.InteropServices;
using System.Windows;

namespace Multitool.NTInterop
{
    public class CursorPosition
    {
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out ulong _lpPoint);

        public Point GetCursorPosition()
        {
            GetCursorPos(out ulong pos);
            int x = (int)(pos & 0x00000000FFFFFFFF);
            ulong y = (pos & 0xFFFFFFFF00000000) >> 32;
            return new Point(x, y);
        }

        public void GetCursorPosition(out int x, out int y)
        {
            GetCursorPos(out ulong pos);
            x = (int)(pos & 0x00000000FFFFFFFF);
            y = (int)((pos & 0xFFFFFFFF00000000) >> 32);
        }
    }
}
