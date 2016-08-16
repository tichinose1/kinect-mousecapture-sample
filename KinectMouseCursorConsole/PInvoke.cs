using System.Runtime.InteropServices;

namespace KinectMouseCursorConsole
{
    public static class PInvoke
    {
        const int MOUSEEVENTF_LEFTDOWN = 0x2;
        const int MOUSEEVENTF_LEFTUP = 0x4;

        public static void PerformClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static void PerformMoveCursor(int x, int y)
        {
            SetCursorPos(x, y);
        }

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("User32.dll")]
        static extern bool SetCursorPos(int X, int Y);
    }

}
