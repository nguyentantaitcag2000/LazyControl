using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        public Form1()
        {
            InitializeComponent();

            highlightForm = new CursorHighlightForm();

            this.Icon = new Icon("favicon.ico"); // Đảm bảo file tồn tại và đúng định dạng

            cursorIndicator = new CursorIndicatorForm();

            keyboardHook = new KeyboardHook();
            keyboardHook.KeyDown += OnKeyDown;
            keyboardHook.KeyUp += OnKeyUp;
            keyboardHook.Start();

            systemKeyCheckTimer = new System.Windows.Forms.Timer();
            systemKeyCheckTimer.Interval = 300; // 300ms kiểm tra một lần
            systemKeyCheckTimer.Tick += CheckSystemKeysReleased;
            systemKeyCheckTimer.Start();

            var _ = this.Handle; // Dòng này để trigger cho phép có thể bật tắt chế độ ngay từ lần đầu bật ứng dụng
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Hide(); // Ẩn form ngay khi load
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Win32.RegisterHotKey(this.Handle, 1, 0, (int)Keys.F8);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            Win32.UnregisterHotKey(this.Handle, 1);
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
            keyboardHook.Stop();
            Win32.UnregisterHotKey(this.Handle, 1);
            movementTokenSource?.Cancel();

            if (isLeftMouseDown)
            {
                Win32.LeftMouseUp();
                isLeftMouseDown = false;
            }

            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312) // WM_HOTKEY
            {
                mouseControlEnabled = !mouseControlEnabled;
                this.Text = $"Mouse Control: {(mouseControlEnabled ? "ON" : "OFF")}";

                if (mouseControlEnabled)
                {
                    highlightForm.ShowHighlight();
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
            }

            base.WndProc(ref m);
        }

        private bool OnKeyDown(Keys key)
        {
            if (!mouseControlEnabled) return false;

            // Nếu người dùng nhấn các nút như Ctrl hoặc Windown thì tạm disable đi trong trường hợp này để người dùng sử dụng các chức năng khác, ví dụ như Window + Shift + S
            if ((key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.LWin || key == Keys.RWin) && !isPausedBySystemKey)
            {
                if (mouseControlEnabled)
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
                }
                return false;
            }

            if (key == Keys.S || key == Keys.W || key == Keys.A || key == Keys.D || key == Keys.L)
            {
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
                    return true;

                case Keys.J:
                    if (!isLeftMouseDown)
                    {
                        Win32.LeftMouseDown();
                        isLeftMouseDown = true;
                    }
                    return true;

                case Keys.K:
                    Win32.RightClick();
                    return true;
            }

            return false;
        }

        private bool OnKeyUp(Keys key)
        {
            if (!mouseControlEnabled) return false;

            if (key == Keys.S || key == Keys.W || key == Keys.A || key == Keys.D || key == Keys.L)
            {
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

            if (key == Keys.J)
            {
                if (isLeftMouseDown)
                {
                    Win32.LeftMouseUp();
                    isLeftMouseDown = false;
                }
                return true;
            }

            if (key == Keys.F || key == Keys.K)
            {
                return true;
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
            bool ctrlDown = (GetAsyncKeyState(Keys.LControlKey) & 0x8000) != 0 ||
                            (GetAsyncKeyState(Keys.RControlKey) & 0x8000) != 0;

            bool winDown = (GetAsyncKeyState(Keys.LWin) & 0x8000) != 0 ||
                           (GetAsyncKeyState(Keys.RWin) & 0x8000) != 0;

            if (!ctrlDown && !winDown && isPausedBySystemKey)
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
    }
}
