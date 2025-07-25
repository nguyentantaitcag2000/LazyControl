using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms ;

namespace LazyControl
{
    public class MonitorSwitcher
    {
        private readonly List<FocusBorderForm> focusBorders;
        private System.Windows.Forms.Timer hideBorderTimer;

        public MonitorSwitcher()
        {
            focusBorders = new List<FocusBorderForm>();

            // Tạo 4 form làm viền (top, bottom, left, right)
            for (int i = 0; i < 4; i++)
            {
                focusBorders.Add(new FocusBorderForm());
            }

            hideBorderTimer = new System.Windows.Forms.Timer();
            hideBorderTimer.Interval = 3000; // 3 giây
            hideBorderTimer.Tick += (s, e) =>
            {
                HideFocusBorder();
                hideBorderTimer.Stop();
            };
        }

        /// <summary>
        /// Kích hoạt cửa sổ trên monitor được chỉ định
        /// </summary>
        /// <param name="monitorIndex">Chỉ số monitor (1-based)</param>
        public void ActivateWindowOnMonitor(int monitorIndex)
        {
            if (monitorIndex > Screen.AllScreens.Length)
                return;

            var exceptions = new[] { "Shell_TrayWnd", "Shell_SecondaryTrayWnd", "WorkerW" };
            var windows = GetAllWindows();

            foreach (var window in windows)
            {
                var className = GetWindowClassName(window);

                // Bỏ qua các cửa sổ hệ thống
                if (exceptions.Contains(className))
                    continue;

                var windowMonitor = GetMonitorFromWindow(window);

                if (windowMonitor == monitorIndex)
                {
                    if (!IsWindowActive(window))
                    {
                        Win32WindowManager.SetForegroundWindow(window);
                        Task.Delay(100).Wait(); // Đợi cửa sổ activate hoàn toàn
                    }

                    DrawFocusBorder(window);
                    break;
                }
            }
        }

        /// <summary>
        /// Vẽ viền quanh cửa sổ được chỉ định
        /// </summary>
        /// <param name="hwnd">Handle của cửa sổ</param>
        private void DrawFocusBorder(IntPtr hwnd)
        {
            if (!Win32WindowManager.GetWindowRect(hwnd, out var rect))
                return;

            const int borderSize = 4;
            const int padding = -10;

            var x = rect.Left;
            var y = rect.Top;
            var w = rect.Right - rect.Left;
            var h = rect.Bottom - rect.Top;

            // Top border
            focusBorders[0].ShowBorder(
                x - padding,
                y - padding,
                w + 2 * padding,
                borderSize
            );

            // Bottom border
            focusBorders[1].ShowBorder(
                x - padding,
                y + h + padding - borderSize,
                w + 2 * padding,
                borderSize
            );

            // Left border
            focusBorders[2].ShowBorder(
                x - padding,
                y - padding,
                borderSize,
                h + 2 * padding
            );

            // Right border
            focusBorders[3].ShowBorder(
                x + w + padding - borderSize,
                y - padding,
                borderSize,
                h + 2 * padding
            );

            // Bắt đầu timer để ẩn viền sau 3 giây
            hideBorderTimer.Stop();
            hideBorderTimer.Start();
        }

        /// <summary>
        /// Ẩn tất cả viền focus
        /// </summary>
        private void HideFocusBorder()
        {
            foreach (var border in focusBorders)
            {
                border.Hide();
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả cửa sổ
        /// </summary>
        private List<IntPtr> GetAllWindows()
        {
            var windows = new List<IntPtr>();

            Win32WindowManager.EnumWindows((hwnd, lParam) =>
            {
                if (Win32WindowManager.IsWindowVisible(hwnd) &&
                    !string.IsNullOrEmpty(GetWindowTitle(hwnd)))
                {
                    windows.Add(hwnd);
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary>
        /// Lấy tên class của cửa sổ
        /// </summary>
        private string GetWindowClassName(IntPtr hwnd)
        {
            const int maxLength = 256;
            var className = new System.Text.StringBuilder(maxLength);
            Win32WindowManager.GetClassName(hwnd, className, maxLength);
            return className.ToString();
        }

        /// <summary>
        /// Lấy tiêu đề của cửa sổ
        /// </summary>
        private string GetWindowTitle(IntPtr hwnd)
        {
            const int maxLength = 256;
            var title = new System.Text.StringBuilder(maxLength);
            Win32WindowManager.GetWindowText(hwnd, title, maxLength);
            return title.ToString();
        }

        /// <summary>
        /// Kiểm tra cửa sổ có đang active không
        /// </summary>
        private bool IsWindowActive(IntPtr hwnd)
        {
            return Win32WindowManager.GetForegroundWindow() == hwnd;
        }

        /// <summary>
        /// Lấy chỉ số monitor chứa cửa sổ
        /// </summary>
        private int GetMonitorFromWindow(IntPtr hwnd)
        {
            var monitor = Win32WindowManager.MonitorFromWindow(hwnd, 2); // MONITOR_DEFAULTTONEAREST

            // Chuyển đổi handle thành số thứ tự monitor
            var screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                var screenMonitor = Win32WindowManager.MonitorFromPoint(
                    new Point(screens[i].Bounds.X, screens[i].Bounds.Y), 2);

                if (screenMonitor == monitor)
                    return i + 1; // 1-based index
            }

            return 1; // Fallback to primary monitor
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            hideBorderTimer?.Dispose();

            foreach (var border in focusBorders)
            {
                border?.Dispose();
            }

            focusBorders.Clear();
        }
    }

    /// <summary>
    /// Form dùng để vẽ viền focus
    /// </summary>
    public class FocusBorderForm : Form
    {
        public FocusBorderForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.DoubleBuffer, true);

            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.Red;
            this.Opacity = 0.8;

            // Làm cho form không thể click được
            this.SetStyle(ControlStyles.Selectable, false);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80000 | 0x20; // WS_EX_LAYERED | WS_EX_TRANSPARENT
                return cp;
            }
        }

        protected override bool ShowWithoutActivation => true;

        /// <summary>
        /// Hiển thị viền tại vị trí và kích thước được chỉ định
        /// </summary>
        public void ShowBorder(int x, int y, int width, int height)
        {
            this.SetBounds(x, y, width, height);
            this.Show();
        }
    }

    /// <summary>
    /// Lớp chứa các Win32 API cần thiết cho việc quản lý cửa sổ
    /// </summary>
    public static class Win32WindowManager
    {
        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder className, int maxCount);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromPoint(Point pt, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }

}
