using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

public class KeyboardHook
{
    public event Func<Keys, bool> KeyDown;  // Trả về true để chặn phím
    public event Func<Keys, bool> KeyUp;    // Trả về true để chặn phím

    private IntPtr hookId = IntPtr.Zero;
    private LowLevelKeyboardProc proc;
    private readonly object lockObject = new object();
    private bool isHookActive = false;

    // Timer để tái tạo hook định kỳ nhằm duy trì độ ưu tiên
    private System.Windows.Forms.Timer refreshTimer;

    public void Start()
    {
        lock (lockObject)
        {
            if (isHookActive) return;

            proc = HookCallback; // Giữ delegate để không bị GC
            hookId = SetHook(proc);
            isHookActive = true;

            // Tạo timer để refresh hook mỗi 30 giây để duy trì độ ưu tiên
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 30000; // 30 giây
            refreshTimer.Tick += RefreshHook;
            refreshTimer.Start();
        }
    }

    public void Stop()
    {
        lock (lockObject)
        {
            if (!isHookActive) return;

            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            refreshTimer = null;

            UnhookWindowsHookEx(hookId);
            hookId = IntPtr.Zero;
            isHookActive = false;
        }
    }

    private void RefreshHook(object sender, EventArgs e)
    {
        // Tái tạo hook để duy trì độ ưu tiên cao
        lock (lockObject)
        {
            if (!isHookActive) return;

            UnhookWindowsHookEx(hookId);
            hookId = SetHook(proc);
        }
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;

        // Đặt priority cao cho process để có độ ưu tiên cao hơn
        try
        {
            curProcess.PriorityClass = ProcessPriorityClass.High;
        }
        catch
        {
            // Nếu không có quyền, bỏ qua
        }

        IntPtr hook = SetWindowsHookEx(WH_KEYBOARD_LL, proc,
            GetModuleHandle(curModule.ModuleName), 0);

        if (hook == IntPtr.Zero)
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new Exception($"Failed to install hook. Error code: {errorCode}");
        }

        return hook;
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            try
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                bool suppressKey = false;

                switch ((int)wParam)
                {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        suppressKey = KeyDown?.Invoke(key) ?? false;
                        break;
                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                        suppressKey = KeyUp?.Invoke(key) ?? false;
                        break;
                }

                // Nếu cần chặn phím, trả về 1 thay vì gọi CallNextHookEx
                if (suppressKey)
                {
                    // Thêm một chút delay để đảm bảo các ứng dụng khác không can thiệp
                    return (IntPtr)1;
                }
            }
            catch
            {
                // Bỏ qua lỗi để hook không bị crash
            }
        }

        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }

    // Thêm method để force refresh hook khi cần
    public void ForceRefresh()
    {
        if (isHookActive)
        {
            Stop();
            Thread.Sleep(100); // Đợi một chút
            Start();
        }
    }

    // Thêm method để kiểm tra hook có còn hoạt động không
    public bool IsActive()
    {
        return isHookActive && hookId != IntPtr.Zero;
    }

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}