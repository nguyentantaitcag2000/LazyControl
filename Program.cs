using AutoUpdaterDotNET;

namespace LazyControl
{
    internal static class Program
    {
        // Tên mutex duy nhất cho ứng dụng (nên là duy nhất toàn hệ thống)
        private static Mutex? _mutex;

        [STAThread]
        static void Main()
        {
            bool isNewInstance;

            // Khởi tạo mutex với tên duy nhất
            _mutex = new Mutex(true, "LazyControlAppMutex", out isNewInstance);

            if (!isNewInstance)
            {
                // Nếu instance đã tồn tại, thông báo và thoát
                MessageBox.Show("Application is running", "Notify", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Kiểm tra cập nhật ứng dụng
            AutoUpdater.InstalledVersion = new Version("1.0.0.1");
            AutoUpdater.Start("https://storage-test.lazycodet.com/products/lazycontrol/AutoUpdater.xml");

            // Chạy ứng dụng như bình thường
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayAppContext());

            // Giải phóng mutex sau khi ứng dụng thoát
            _mutex.ReleaseMutex();
            
        }
    }
}
