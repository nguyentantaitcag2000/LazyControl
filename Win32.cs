using System.Drawing;
using System.Runtime.InteropServices;

public static class Win32
{
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll")]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    // 2 Thư viện customize hình dạng cursor

    [DllImport("user32.dll")]
    private static extern IntPtr LoadCursorFromFile(string fileName);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr hCursor);

    // Mouse event flags
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
    private const int MOUSEEVENTF_RIGHTUP = 0x10;
    private const int MOUSEEVENTF_WHEEL = 0x0800;
    private const int MOUSEEVENTF_HWHEEL = 0x01000; // Horizontal wheel flag

    // Scroll wheel values
    private const int WHEEL_DELTA = 120;

    /// <summary>
    /// Click chuột trái (down + up nhanh)
    /// </summary>
    public static void LeftClick()
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    /// <summary>
    /// Nhấn và giữ chuột trái (để bắt đầu drag/select)
    /// </summary>
    public static void LeftMouseDown()
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
    }

    /// <summary>
    /// Thả chuột trái (để kết thúc drag/select)
    /// </summary>
    public static void LeftMouseUp()
    {
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    /// <summary>
    /// Click chuột phải
    /// </summary>
    public static void RightClick()
    {
        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
    }

    /// <summary>
    /// Scroll lên
    /// </summary>
    public static void ScrollUp()
    {
        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, WHEEL_DELTA, 0);
    }

    /// <summary>
    /// Scroll xuống
    /// </summary>
    public static void ScrollDown()
    {
        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -WHEEL_DELTA, 0);
    }

    /// <summary>
    /// Scroll trái
    /// </summary>
    public static void ScrollLeft()
    {
        mouse_event(MOUSEEVENTF_HWHEEL, 0, 0, -WHEEL_DELTA, 0);
    }

    /// <summary>
    /// Scroll phải
    /// </summary>
    public static void ScrollRight()
    {
        mouse_event(MOUSEEVENTF_HWHEEL, 0, 0, WHEEL_DELTA, 0);
    }
}