using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LazyControl
{
    public partial class CursorIndicatorForm : Form
    {
        public CursorIndicatorForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.Magenta; // Màu sẽ được dùng làm màu trong suốt
            this.TransparencyKey = Color.Magenta;

            this.Width = 50;
            this.Height = 50;

            this.StartPosition = FormStartPosition.Manual;

            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(100, Color.Red)))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, 0, 0, this.Width, this.Height);
            }
        }
    }
}
