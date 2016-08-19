using System;
using System.Runtime.InteropServices;

namespace KinectMouseCursorConsole
{
    public static class PInvoke
    {
        const int MOUSEEVENTF_LEFTDOWN = 0x2;
        const int MOUSEEVENTF_LEFTUP = 0x4;
        const int LOGPIXELSX = 88;
        const int LOGPIXELSY = 90;
        static readonly IntPtr hdc = GetDC(IntPtr.Zero);

        public static void PerformClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static void PerformMoveCursor(int x, int y) => SetCursorPos(x, y);

        public static int GetDpiX() => GetDeviceCaps(hdc, LOGPIXELSX);

        public static int GetDpiY() => GetDeviceCaps(hdc, LOGPIXELSY);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("User32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
    }
}
