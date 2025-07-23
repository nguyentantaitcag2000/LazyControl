using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class KeyboardHook
{
    public event Func<Keys, bool> KeyDown;  // Trả về true để chặn phím
    public event Func<Keys, bool> KeyUp;    // Trả về true để chặn phím

    private IntPtr hookId = IntPtr.Zero;
    private LowLevelKeyboardProc proc;

    public void Start()
    {
        proc = HookCallback; // Giữ delegate để không bị GC
        hookId = SetHook(proc);
    }

    public void Stop()
    {
        UnhookWindowsHookEx(hookId);
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
            GetModuleHandle(curModule.ModuleName), 0);
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
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
                return (IntPtr)1;
        }

        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}