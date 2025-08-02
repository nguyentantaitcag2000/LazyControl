using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LazyControl
{
    public class TrayAppContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private SettingsForm settingsForm;
        private Form1 form1 { get; set; }

        public TrayAppContext()
        {
            // Tạo form nhưng chưa hiển thị
            form1 = new Form1();
            settingsForm = new SettingsForm();
            form1.FormClosing += (s, e) =>
            {
                // Khi đóng form, ẩn đi chứ không thoát app
                e.Cancel = true;
                form1.Hide();
            };

            // Tạo context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Settings", null, OnSettingsClick);
            contextMenu.Items.Add("Quit", null, OnQuitClick);
            var assembly = Assembly.GetExecutingAssembly();
            using Stream? iconStream = assembly.GetManifestResourceStream("LazyControl.favicon.ico");
            // Tạo tray icon
            trayIcon = new NotifyIcon()
            {
                //Icon = SystemIcons.Application, // Bạn có thể dùng icon riêng
                Icon = new Icon(iconStream),
                ContextMenuStrip = contextMenu,
                Visible = true,
                Text = "LazyControl v" + Configuration.VERSION
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
