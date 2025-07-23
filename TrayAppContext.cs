using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyControl
{
    public class TrayAppContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private Form1 settingsForm;

        public TrayAppContext()
        {
            // Tạo form nhưng chưa hiển thị
            settingsForm = new Form1();
            settingsForm.FormClosing += (s, e) =>
            {
                // Khi đóng form, ẩn đi chứ không thoát app
                e.Cancel = true;
                settingsForm.Hide();
            };

            // Tạo context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Settings", null, OnSettingsClick);
            contextMenu.Items.Add("Quit", null, OnQuitClick);

            // Tạo tray icon
            trayIcon = new NotifyIcon()
            {
                //Icon = SystemIcons.Application, // Bạn có thể dùng icon riêng
                Icon = new Icon("favicon.ico"),
                ContextMenuStrip = contextMenu,
                Visible = true,
                Text = "LazyControl"
            };

            trayIcon.DoubleClick += OnSettingsClick; // Nháy đúp cũng mở form
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            if (!settingsForm.Visible)
            {
                settingsForm.Show();
                settingsForm.WindowState = FormWindowState.Normal;
            }
            settingsForm.BringToFront();
        }

        private void OnQuitClick(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }
    }

}
