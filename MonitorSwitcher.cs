using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LazyControl
{
    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; }
        public string ClassName { get; set; }
        public int Area { get; set; }
        public bool IsMaximized { get; set; }
        public bool IsMinimized { get; set; }
        public Win32WindowManager.RECT Bounds { get; set; }
        public int MonitorIndex { get; set; }
    }

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
        /// Kích hoạt cửa sổ tốt nhất trên monitor được chỉ định
        /// </summary>
        /// <param name="monitorIndex">Chỉ số monitor (1-based)</param>
        public void ActivateWindowOnMonitor(int monitorIndex)
        {
            if (monitorIndex > Screen.AllScreens.Length)
                return;

            var bestWindow = FindBestWindowOnMonitor(monitorIndex);

            if (bestWindow != null)
            {
                // Nếu cửa sổ bị minimize, restore nó trước
                if (bestWindow.IsMinimized)
                {
                    Win32WindowManager.ShowWindow(bestWindow.Handle, 9); // SW_RESTORE
                    Task.Delay(200).Wait(); // Đợi restore hoàn tất
                }

                // Focus cửa sổ
                if (!IsWindowActive(bestWindow.Handle))
                {
                    Win32WindowManager.SetForegroundWindow(bestWindow.Handle);
                    Task.Delay(100).Wait(); // Đợi cửa sổ activate hoàn toàn
                }

                DrawFocusBorder(bestWindow.Handle);
            }
            else
            {
                // Nếu không tìm thấy cửa sổ nào, hiển thị thông báo trên monitor đó
                ShowNoWindowFoundIndicator(monitorIndex);
            }
        }

        /// <summary>
        /// Tìm cửa sổ tốt nhất trên monitor chỉ định
        /// </summary>
        private WindowInfo FindBestWindowOnMonitor(int targetMonitorIndex)
        {
            var windows = GetAllValidWindows()
                .Where(w => w.MonitorIndex == targetMonitorIndex)
                .ToList();

            if (!windows.Any())
                return null;

            // Ưu tiên 1: Cửa sổ đang được maximize
            var maximizedWindows = windows.Where(w => w.IsMaximized && !w.IsMinimized).ToList();
            if (maximizedWindows.Any())
            {
                return maximizedWindows.OrderByDescending(w => w.Area).First();
            }

            // Ưu tiên 2: Cửa sổ có diện tích lớn nhất (không bị minimize)
            var visibleWindows = windows.Where(w => !w.IsMinimized).ToList();
            if (visibleWindows.Any())
            {
                // Tìm cửa sổ có diện tích lớn nhất, nhưng phải có kích thước hợp lý
                var suitableWindows = visibleWindows.Where(w => w.Area > 50000).ToList(); // Ít nhất 200x250 pixels

                if (suitableWindows.Any())
                {
                    return suitableWindows.OrderByDescending(w => w.Area).First();
                }

                // Nếu không có cửa sổ đủ lớn, chọn cửa sổ lớn nhất có sẵn
                return visibleWindows.OrderByDescending(w => w.Area).First();
            }

            // Ưu tiên 3: Nếu tất cả đều minimize, chọn cái có diện tích lớn nhất để restore
            return windows.OrderByDescending(w => w.Area).First();
        }

        /// <summary>
        /// Lấy danh sách tất cả cửa sổ hợp lệ với thông tin chi tiết
        /// </summary>
        private List<WindowInfo> GetAllValidWindows()
        {
            var windows = new List<WindowInfo>();
            var exceptions = new[] {
                "Shell_TrayWnd", "Shell_SecondaryTrayWnd", "WorkerW",
                "Progman", "Button", "Static", "DV2ControlHost",
                "#32770", "CabinetWClass", "ExploreWClass" // Thêm một số class cần bỏ qua
            };

            Win32WindowManager.EnumWindows((hwnd, lParam) =>
            {
                try
                {
                    if (!Win32WindowManager.IsWindowVisible(hwnd))
                        return true;

                    var title = GetWindowTitle(hwnd);
                    var className = GetWindowClassName(hwnd);

                    // Bỏ qua cửa sổ không có title hoặc có class name trong danh sách ngoại lệ
                    if (string.IsNullOrWhiteSpace(title) || exceptions.Contains(className))
                        return true;

                    // Bỏ qua các cửa sổ có title quá ngắn (có thể là dialog hoặc popup)
                    if (title.Length < 3)
                        return true;

                    if (!Win32WindowManager.GetWindowRect(hwnd, out var rect))
                        return true;

                    var width = rect.Right - rect.Left;
                    var height = rect.Bottom - rect.Top;
                    var area = width * height;

                    // Bỏ qua cửa sổ quá nhỏ (có thể là tooltip, notification, etc.)
                    if (width < 100 || height < 50)
                        return true;

                    var windowInfo = new WindowInfo
                    {
                        Handle = hwnd,
                        Title = title,
                        ClassName = className,
                        Area = area,
                        Bounds = rect,
                        IsMaximized = Win32WindowManager.IsZoomed(hwnd),
                        IsMinimized = Win32WindowManager.IsIconic(hwnd),
                        MonitorIndex = GetMonitorFromWindow(hwnd)
                    };

                    windows.Add(windowInfo);
                }
                catch
                {
                    // Ignore any errors and continue enumeration
                }

                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary>
        /// Hiển thị indicator khi không tìm thấy cửa sổ nào
        /// </summary>
        private void ShowNoWindowFoundIndicator(int monitorIndex)
        {
            if (monitorIndex > Screen.AllScreens.Length)
                return;

            var screen = Screen.AllScreens[monitorIndex - 1];
            var centerX = screen.Bounds.X + screen.Bounds.Width / 2;
            var centerY = screen.Bounds.Y + screen.Bounds.Height / 2;

            // Hiển thị một hình chữ nhật nhỏ ở giữa màn hình
            focusBorders[0].ShowBorder(centerX - 100, centerY - 2, 200, 4);
            focusBorders[1].ShowBorder(centerX - 100, centerY + 2, 200, 4);
            focusBorders[2].ShowBorder(centerX - 102, centerY - 50, 4, 100);
            focusBorders[3].ShowBorder(centerX + 98, centerY - 50, 4, 100);

            // Tự động ẩn sau 1 giây
            hideBorderTimer.Stop();
            hideBorderTimer.Interval = 1000;
            hideBorderTimer.Start();
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
            hideBorderTimer.Interval = 3000;
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

        // THÊM CÁC API MỚI ĐỂ KIỂM TRA TRẠNG THÁI CỬA SỔ
        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hWnd); // Kiểm tra maximized

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd); // Kiểm tra minimized

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); // Show/hide/restore window

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