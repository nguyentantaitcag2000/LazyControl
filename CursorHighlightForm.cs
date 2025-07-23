using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LazyControl
{
    public partial class CursorHighlightForm : Form
    {
        private System.Windows.Forms.Timer followTimer;

        private const int CircleDiameter = 80; // chỉnh kích thước tùy ý

        public CursorHighlightForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.Width = this.Height = CircleDiameter + 4; // thêm padding viền
            this.Opacity = 0.2;
            this.DoubleBuffered = true;

            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            followTimer = new System.Windows.Forms.Timer();
            followTimer.Interval = 30;
            followTimer.Tick += (s, e) => FollowCursor();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80000 | 0x20; // WS_EX_LAYERED | WS_EX_TRANSPARENT
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //using (Pen pen = new Pen(Color.Red, 4))
            //{
            //    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //    // Vẽ vòng tròn nằm chính giữa, padding 2px mỗi bên
            //    e.Graphics.DrawEllipse(pen, 2, 2, CircleDiameter, CircleDiameter);
            //}

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (Brush brush = new SolidBrush(Color.FromArgb(250, Color.Red)))
            {
                e.Graphics.FillEllipse(brush, 2, 2, CircleDiameter, CircleDiameter);
            }
        }

        public void ShowHighlight()
        {
            this.Show();
            followTimer.Start();
        }

        public void HideHighlight()
        {
            followTimer.Stop();
            this.Hide();
        }

        private void FollowCursor()
        {
            Point cursorPos = Cursor.Position;

            // Đặt location sao cho cursor nằm chính giữa vòng tròn (tâm là 2 + radius)
            int radius = CircleDiameter / 2;
            int offset = 2 + radius;

            this.Location = new Point(
                cursorPos.X - offset,
                cursorPos.Y - offset
            );
        }


    }
}
