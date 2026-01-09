using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LazyControl
{
    public partial class Form1 : Form
    {
        private KeyboardHook keyboardHook;
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private CancellationTokenSource movementTokenSource;
        private bool mouseControlEnabled = false;
        private bool isLeftMouseDown = false; // Trạng thái chuột trái
        private readonly object lockObject = new object();

        private CursorIndicatorForm cursorIndicator;
        private CancellationTokenSource indicatorTokenSource;

        private IntPtr customCursor = IntPtr.Zero;
        private Cursor previousCursor;

        private CursorHighlightForm highlightForm;

        private bool wasMouseControlEnabledBeforeSystemKey = false;
        private bool isPausedBySystemKey = false;
        private System.Windows.Forms.Timer systemKeyCheckTimer;

        private MonitorSwitcher monitorSwitcher;

        // Thêm biến để theo dõi trạng thái phím ESC
        private bool isEscPressed = false;

        // Thêm timer để kiểm tra và tái tạo hook nếu cần
        private System.Windows.Forms.Timer hookHealthCheckTimer;

        // Thêm biến để track trạng thái các phím modifier và toggle key
        private bool isCtrlPressed = false;
        private bool isShiftPressed = false;
        private bool isAltPressed = false;
        private Dictionary<Keys, bool> toggleKeyStates = new Dictionary<Keys, bool>(); // Track trạng thái các phím trong hotkey
        private bool toggleProcessed = false; // Để tránh xử lý nhiều lần

        // Lưu settings hiện tại
        private AppSettings currentSettings;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        public Form1()
        {
            InitializeComponent();

            // Load settings ngay từ đầu
            currentSettings = SettingsManager.LoadSettings();

            highlightForm = new CursorHighlightForm();
            var assembly = Assembly.GetExecutingAssembly();
            using Stream? iconStream = assembly.GetManifestResourceStream("LazyControl.favicon.ico");
            this.Icon = new Icon(iconStream); // Đảm bảo file tồn tại và đúng định dạng

            cursorIndicator = new CursorIndicatorForm();

            InitializeKeyboardHook();

            systemKeyCheckTimer = new System.Windows.Forms.Timer();
            systemKeyCheckTimer.Interval = 300; // 300ms kiểm tra một lần
            systemKeyCheckTimer.Tick += CheckSystemKeysReleased;
            systemKeyCheckTimer.Start();

            // Timer để kiểm tra sức khỏe của hook
            hookHealthCheckTimer = new System.Windows.Forms.Timer();
            hookHealthCheckTimer.Interval = 1000; // 1 giây kiểm tra một lần
            hookHealthCheckTimer.Tick += CheckHookHealth;
            hookHealthCheckTimer.Start();

            monitorSwitcher = new MonitorSwitcher();

            AppInstaller.EnsureInstalled();

            var _ = this.Handle; // Dòng này để trigger cho phép có thể bật tắt chế độ ngay từ lần đầu bật ứng dụng
        }

        private void InitializeKeyboardHook()
        {
            try
            {
                keyboardHook = new KeyboardHook();
                keyboardHook.KeyDown += OnKeyDown;
                keyboardHook.KeyUp += OnKeyUp;
                keyboardHook.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize keyboard hook: {ex.Message}", "LazyControl Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckHookHealth(object sender, EventArgs e)
        {
            // Kiểm tra xem hook có còn hoạt động không
            if (keyboardHook != null && !keyboardHook.IsActive())
            {
                // Hook bị mất, tái tạo lại
                try
                {
                    keyboardHook.Stop();
                    keyboardHook.Start();

                    // Hiển thị thông báo ngắn gọn
                    this.Text = "Hook Restored - " + (mouseControlEnabled ? "Mouse Control: ON" : "Mouse Control: OFF");
                }
                catch
                {
                    // Nếu không thể tái tạo, thử tạo mới hoàn toàn
                    try
                    {
                        keyboardHook?.Stop();
                        InitializeKeyboardHook();
                    }
                    catch
                    {
                        // Nếu vẫn không được, tạm dừng check một lúc
                        hookHealthCheckTimer.Interval = 30000; // 30 giây
                    }
                }
            }
        }

        [Flags]
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Hide(); // Ẩn form ngay khi load
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Không cần đăng ký hotkey vì xử lý trực tiếp trong keyboard hook
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Nếu người dùng bấm nút X, chỉ ẩn form
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }

            // Nếu thực sự thoát, cleanup
            hookHealthCheckTimer?.Stop();
            hookHealthCheckTimer?.Dispose();

            keyboardHook?.Stop();
            movementTokenSource?.Cancel();

            if (isLeftMouseDown)
            {
                Win32.LeftMouseUp();
                isLeftMouseDown = false;
            }

            monitorSwitcher?.Dispose();

            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
        }

        // Hàm helper để extract các modifier keys từ Keys combination
        private (bool hasCtrl, bool hasShift, bool hasAlt, Keys baseKey) ParseKeyCombination(Keys combination)
        {
            bool hasCtrl = (combination & Keys.Control) == Keys.Control;
            bool hasShift = (combination & Keys.Shift) == Keys.Shift;
            bool hasAlt = (combination & Keys.Alt) == Keys.Alt;
            Keys baseKey = combination & ~Keys.Modifiers; // Loại bỏ tất cả modifier để lấy phím gốc

            return (hasCtrl, hasShift, hasAlt, baseKey);
        }

        // Hàm kiểm tra xem hotkey toggle có đang được nhấn không
        private bool IsToggleHotkeyPressed()
        {
            var (hasCtrl, hasShift, hasAlt, baseKey) = ParseKeyCombination(currentSettings.ToggleMouseMode);

            // Kiểm tra các modifier keys có khớp không
            bool modifiersMatch = (hasCtrl == isCtrlPressed) &&
                                  (hasShift == isShiftPressed) &&
                                  (hasAlt == isAltPressed);

            // Kiểm tra base key có được nhấn không
            bool baseKeyPressed = toggleKeyStates.ContainsKey(baseKey) && toggleKeyStates[baseKey];

            return modifiersMatch && baseKeyPressed;
        }

        // THÊM HÀM HELPER ĐỂ KIỂM TRA CÓ PHẢI SYSTEM HOTKEY KHÔNG
        private bool IsSystemHotkey(Keys key)
        {
            // Kiểm tra các tổ hợp phím hệ thống quan trọng
            if (isCtrlPressed)
            {
                // Các phím thường dùng với Ctrl
                if (key == Keys.C || key == Keys.V || key == Keys.X || key == Keys.Z ||
                    key == Keys.Y || key == Keys.A || key == Keys.F || key == Keys.H ||
                    key == Keys.N || key == Keys.O || key == Keys.P || key == Keys.R ||
                    key == Keys.S || key == Keys.T || key == Keys.W || key == Keys.Tab ||
                    key == Keys.F4 || key == Keys.Enter || key == Keys.Home || key == Keys.End ||
                    key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down)
                {
                    return true;
                }

                // Kiểm tra Ctrl+Shift combinations
                if (isShiftPressed)
                {
                    // Ctrl+Shift+N, Ctrl+Shift+T, Ctrl+Shift+Tab, etc.
                    if (key == Keys.N || key == Keys.T || key == Keys.Tab || key == Keys.Escape ||
                        key == Keys.Delete || key == Keys.Z || key == Keys.Enter)
                    {
                        return true;
                    }
                }
            }

            // Alt combinations
            if (isAltPressed)
            {
                return true; // Cho phép tất cả Alt combinations
            }

            return false;
        }

        private bool OnKeyDown(Keys key)
        {
            // Track trạng thái các modifier keys
            if (key == Keys.LControlKey || key == Keys.RControlKey)
            {
                isCtrlPressed = true;
                toggleProcessed = false; // Reset flag khi nhấn Ctrl mới
                return false; // Cho phép Ctrl hoạt động bình thường
            }

            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
            {
                isShiftPressed = true;
                return false; // Cho phép Shift hoạt động bình thường
            }

            if (key == Keys.LMenu || key == Keys.RMenu) // Alt keys
            {
                isAltPressed = true;
                return false; // Cho phép Alt hoạt động bình thường
            }

            // Lấy base key từ toggle hotkey
            var (_, _, _, toggleBaseKey) = ParseKeyCombination(currentSettings.ToggleMouseMode);

            // Track trạng thái của base key trong toggle hotkey
            if (key == toggleBaseKey)
            {
                toggleKeyStates[key] = true;

                // Kiểm tra xem có phải đang nhấn hotkey toggle không
                if (IsToggleHotkeyPressed() && !toggleProcessed)
                {
                    toggleProcessed = true; // Đánh dấu đã xử lý để tránh lặp

                    // Toggle mouse control
                    mouseControlEnabled = !mouseControlEnabled;
                    this.Text = $"Mouse Control: {(mouseControlEnabled ? "ON" : "OFF")}";

                    if (mouseControlEnabled)
                    {
                        highlightForm.ShowHighlight();
                        // Reset trạng thái pause nếu có
                        isPausedBySystemKey = false;
                        wasMouseControlEnabledBeforeSystemKey = false;

                        // Force refresh hook để đảm bảo nó hoạt động
                        keyboardHook?.ForceRefresh();
                    }
                    else
                    {
                        highlightForm.HideHighlight();

                        lock (lockObject)
                        {
                            pressedKeys.Clear();
                            movementTokenSource?.Cancel();

                            if (isLeftMouseDown)
                            {
                                Win32.LeftMouseUp();
                                isLeftMouseDown = false;
                            }
                        }
                    }

                    return true; // Chặn tổ hợp phím toggle
                }

                // Nếu đã xử lý toggle, chặn key để tránh side effect
                if (toggleProcessed)
                {
                    return true;
                }
            }

            // Nếu không phải toggle hotkey, xử lý như mouse click (nếu là J và mouse control bật)
            if (key == Keys.J && !IsToggleHotkeyPressed() && mouseControlEnabled && !isLeftMouseDown)
            {
                Win32.LeftMouseDown();
                isLeftMouseDown = true;
                return true;
            }

            // Xử lý phím ESC để theo dõi trạng thái
            if (key == Keys.Escape)
            {
                isEscPressed = true;
                return false; // Cho phép ESC hoạt động bình thường NHƯNG vẫn track
            }

            // Xử lý ESC + F1/F2 cho chuyển monitor
            if (isEscPressed && (key == Keys.F1 || key == Keys.F2))
            {
                if (key == Keys.F1)
                {
                    monitorSwitcher.ActivateWindowOnMonitor(currentSettings.EscF1);
                }
                else if (key == Keys.F2)
                {
                    monitorSwitcher.ActivateWindowOnMonitor(currentSettings.EscF2);
                }
                return true; // Chặn phím F1/F2 khi có ESC
            }

            // Xử lý Ctrl + F7/F8 cho điều khiển âm lượng
            if (isCtrlPressed && (key == Keys.F7 || key == Keys.F8))
            {
                if (key == Keys.F7)
                {
                    Win32.VolumeDown();
                }
                else if (key == Keys.F8)
                {
                    Win32.VolumeUp();
                }
                return true; // Chặn phím F7/F8 khi có Ctrl
            }

            // Kiểm tra mouseControlEnabled TRƯỚC khi xử lý các phím di chuyển
            if (!mouseControlEnabled)
            {
                // Nếu mouse control tắt, chỉ cho phép các phím hệ thống hoạt động
                return false;
            }

            // KIỂM TRA SYSTEM HOTKEY TRƯỚC KHI XỬ LÝ CÁC PHÍM DI CHUYỂN
            if (IsSystemHotkey(key))
            {
                return false; // Không chặn các tổ hợp phím hệ thống
            }

            // Nếu giữ Alt + D, bỏ qua để hệ điều hành xử lý
            if (isAltPressed && key == Keys.D)
            {
                return false;
            }

            // Xử lý Windows key - disable mouse control tạm thời
            if ((key == Keys.LWin || key == Keys.RWin) && !isPausedBySystemKey)
            {
                wasMouseControlEnabledBeforeSystemKey = true;
                mouseControlEnabled = false;
                isPausedBySystemKey = true;

                this.Text = "Mouse Control: OFF (Paused)";
                highlightForm.HideHighlight();

                lock (lockObject)
                {
                    pressedKeys.Clear();
                    movementTokenSource?.Cancel();

                    if (isLeftMouseDown)
                    {
                        Win32.LeftMouseUp();
                        isLeftMouseDown = false;
                    }
                }

                return false;
            }

            // Xử lý phím di chuyển - CHỈ KHI KHÔNG CÓ MODIFIER KEYS
            if (key == Keys.S || key == Keys.W || key == Keys.A || key == Keys.D || key == Keys.L)
            {
                // Nếu có bất kỳ modifier key nào, cho phép hệ thống xử lý
                if (isCtrlPressed || isShiftPressed || isAltPressed)
                {
                    return false; // Cho phép tổ hợp phím với modifier hoạt động bình thường
                }

                lock (lockObject)
                {
                    bool wasEmpty = pressedKeys.Count == 0;
                    pressedKeys.Add(key);

                    if (wasEmpty)
                    {
                        movementTokenSource = new CancellationTokenSource();
                        Task.Run(() => MoveMouse(movementTokenSource.Token));
                    }
                }
                return true;
            }

            switch (key)
            {
                case Keys.F:
                    // Chỉ chặn F khi không có modifier keys
                    if (!isCtrlPressed && !isShiftPressed && !isAltPressed)
                    {
                        return true;
                    }
                    return false;

                case Keys.K:
                    // Chỉ chặn K khi không có modifier keys
                    if (!isCtrlPressed && !isShiftPressed && !isAltPressed)
                    {
                        Win32.RightClick();
                        return true;
                    }
                    return false;

                case Keys.N:
                    // Chỉ chặn N khi không có modifier keys
                    if (!isCtrlPressed && !isShiftPressed && !isAltPressed)
                    {
                        Win32.MiddleClick();
                        return true;
                    }
                    return false;
            }

            return false;
        }

        private bool OnKeyUp(Keys key)
        {
            // Track trạng thái các modifier keys
            if (key == Keys.LControlKey || key == Keys.RControlKey)
            {
                isCtrlPressed = false;
                toggleProcessed = false; // Reset khi thả modifier
                return false;
            }

            if (key == Keys.LShiftKey || key == Keys.RShiftKey)
            {
                isShiftPressed = false;
                toggleProcessed = false;
                return false;
            }

            if (key == Keys.LMenu || key == Keys.RMenu) // Alt keys
            {
                isAltPressed = false;
                toggleProcessed = false;
                return false;
            }

            // Lấy base key từ toggle hotkey
            var (_, _, _, toggleBaseKey) = ParseKeyCombination(currentSettings.ToggleMouseMode);

            // Track trạng thái của base key
            if (key == toggleBaseKey)
            {
                toggleKeyStates[key] = false;

                // Nếu đã xử lý toggle, chặn để tránh side effect
                if (toggleProcessed)
                {
                    return true;
                }
            }

            // Nếu là J và không phải toggle hotkey, xử lý mouse up
            if (key == Keys.J && !IsToggleHotkeyPressed() && mouseControlEnabled && isLeftMouseDown)
            {
                Win32.LeftMouseUp();
                isLeftMouseDown = false;
                return true;
            }
            
            // Reset trạng thái ESC khi thả phím
            if (key == Keys.Escape)
            {
                isEscPressed = false;
                return false; // Vẫn cho phép ESC hoạt động bình thường
            }

            // Xử lý F1, F2, F7, F8 khi thả
            if ((key == Keys.F1 || key == Keys.F2 || key == Keys.F7 || key == Keys.F8) && !isEscPressed)
            {
                return false;
            }

            if (!mouseControlEnabled) return false;

            if (key == Keys.S || key == Keys.W || key == Keys.A || key == Keys.D || key == Keys.L)
            {
                // Nếu có modifier keys, không xử lý như phím di chuyển
                if (isCtrlPressed || isShiftPressed || isAltPressed)
                {
                    return false;
                }

                lock (lockObject)
                {
                    pressedKeys.Remove(key);

                    if (pressedKeys.Count == 0)
                    {
                        movementTokenSource?.Cancel();
                    }
                }
                return true;
            }

            // Chỉ chặn các phím chức năng khi không có modifier keys
            if (key == Keys.F || key == Keys.K || key == Keys.N)
            {
                if (!isCtrlPressed && !isShiftPressed && !isAltPressed)
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        private async Task MoveMouse(CancellationToken token)
        {
            double speed = 2.0;
            int delay = 20;
            double maxSpeed = 30.0;

            while (!token.IsCancellationRequested)
            {
                double dx = 0, dy = 0;
                bool shouldScroll = false;
                bool scrollUp = false;
                bool scrollLeft = false;
                bool scrollRight = false;

                lock (lockObject)
                {
                    bool hasL = pressedKeys.Contains(Keys.L);
                    bool hasW = pressedKeys.Contains(Keys.W);
                    bool hasS = pressedKeys.Contains(Keys.S);
                    bool hasA = pressedKeys.Contains(Keys.A);
                    bool hasD = pressedKeys.Contains(Keys.D);

                    // Kiểm tra scroll dọc (L+W hoặc L+S)
                    if (hasL && hasW && !hasS && !hasA && !hasD)
                    {
                        shouldScroll = true;
                        scrollUp = true;
                    }
                    else if (hasL && hasS && !hasW && !hasA && !hasD)
                    {
                        shouldScroll = true;
                        scrollUp = false;
                    }
                    // Kiểm tra scroll ngang (L+A hoặc L+D)
                    else if (hasL && hasA && !hasW && !hasS && !hasD)
                    {
                        shouldScroll = true;
                        scrollLeft = true;
                    }
                    else if (hasL && hasD && !hasW && !hasS && !hasA)
                    {
                        shouldScroll = true;
                        scrollRight = true;
                    }
                    // Nếu không có L, thì di chuyển chuột bình thường
                    else if (!hasL)
                    {
                        foreach (var key in pressedKeys)
                        {
                            switch (key)
                            {
                                case Keys.S: dy += 1; break;
                                case Keys.W: dy -= 1; break;
                                case Keys.A: dx -= 1; break;
                                case Keys.D: dx += 1; break;
                            }
                        }
                    }

                    if (pressedKeys.Count == 0)
                        break;
                }

                // Thực hiện scroll hoặc di chuyển chuột
                if (shouldScroll)
                {
                    if (scrollUp)
                        Win32.ScrollUp();
                    else if (!scrollUp && !scrollLeft && !scrollRight)
                        Win32.ScrollDown();
                    else if (scrollLeft)
                        Win32.ScrollLeft();
                    else if (scrollRight)
                        Win32.ScrollRight();
                }
                else if (dx != 0 || dy != 0)
                {
                    if (dx != 0 && dy != 0)
                    {
                        double normalizer = 1.0 / Math.Sqrt(2);
                        dx *= normalizer;
                        dy *= normalizer;
                    }

                    if (Win32.GetCursorPos(out Point pos))
                    {
                        int newX = pos.X + (int)(dx * speed);
                        int newY = pos.Y + (int)(dy * speed);
                        Win32.SetCursorPos(newX, newY);
                    }

                    if (speed < maxSpeed)
                        speed += 0.5;
                    if (delay > 5)
                        delay -= 1;
                }

                await Task.Delay(delay);
            }
        }

        private void CheckSystemKeysReleased(object sender, EventArgs e)
        {
            bool winDown = (GetAsyncKeyState(Keys.LWin) & 0x8000) != 0 ||
                           (GetAsyncKeyState(Keys.RWin) & 0x8000) != 0;

            if (!winDown && isPausedBySystemKey)
            {
                isPausedBySystemKey = false;

                if (wasMouseControlEnabledBeforeSystemKey)
                {
                    mouseControlEnabled = true;
                    wasMouseControlEnabledBeforeSystemKey = false;

                    this.Text = "Mouse Control: ON";
                    highlightForm.ShowHighlight();
                }
            }
        }

        // Thêm method public để reload settings khi user thay đổi trong SettingsForm
        public void ReloadSettings()
        {
            currentSettings = SettingsManager.LoadSettings();
            toggleProcessed = false;
            toggleKeyStates.Clear();
        }
    }
}