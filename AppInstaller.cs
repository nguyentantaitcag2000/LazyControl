using Microsoft.Win32;
using System.Diagnostics;
using Helper;

public static class AppInstaller
{
    private const string AppName = "LazyControl";

    /// <summary>
    /// 1. Vì project này sẽ xuất ra dạng portable, nên là gì mà người dùng download về và chạy lên luôn file .exe thì nó sẽ 
    /// đăng kí đường dẫn startup tại thư mục đó luôn thì không hay, ta sẽ có 1 bước chuyển file .exe này vào 1 nơi an toàn 
    /// rồi mới đăng kí, lúc này họ xoá đi thư mục đã download cũng không sao
    /// 
    /// <br/><br/>
    /// 
    /// 2. Vì project là dạng portable nên mặc định nó sẽ không tự tạo shorcut ở Start Menu, nên ta tự tạo cái này
    /// </summary>
    public static void EnsureInstalled()
    {
        string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppName
        );

        string targetExePath = Path.Combine(appDataPath, $"{AppName}.exe");
        string currentExePath = Application.ExecutablePath;

        // Nếu đang không chạy từ AppData thì copy và chạy lại từ đó
        if (!currentExePath.Equals(targetExePath, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (!Directory.Exists(appDataPath))
                    Directory.CreateDirectory(appDataPath);

                System.IO.File.Copy(currentExePath, targetExePath, true);

                // Chạy bản mới
                Process.Start(new ProcessStartInfo
                {
                    FileName = targetExePath,
                    UseShellExecute = true
                });

                // Thoát app hiện tại
                #if !DEBUG
                    Environment.Exit(0);
                #endif
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể cài đặt ứng dụng:\n" + ex.Message);
            }
        }
        CreateStartMenuShortcut();
        RegisterInStartup(true);
    }

    /// <summary>
    /// Sau khi chạy hàm này nó sẽ lưu file startup ở registry, có thể tìm nó ở:
    /// 1. Mở Registry Editor (Win + R, gõ regedit)
    /// 2. Đi tới khóa: HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
    /// 3. Tìm giá trị LazyControl
    /// </summary>
    /// <param name="isEnable"></param>
    private static void RegisterInStartup(bool isEnable)
    {
        string exePath = Application.ExecutablePath;

        RegistryKey reg = Registry.CurrentUser.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true
        );

        if (isEnable)
        {
            reg.SetValue(AppName, "\"" + exePath + "\"");
        }
        else
        {
            reg.DeleteValue(AppName, false);
        }
    }

    private static void CreateStartMenuShortcut()
    {
        string shortcutPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            "Programs",
            $"{AppName}.lnk"
        );

        if (!System.IO.File.Exists(shortcutPath))
        {
            try
            {
                string exePath = Application.ExecutablePath;
                ShellShortcut.CreateShortcut(
                    shortcutPath,
                    exePath,
                    Path.GetDirectoryName(exePath),
                    "Ứng dụng điều khiển chuột bằng bàn phím LazyControl",
                    exePath
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tạo shortcut Start Menu:\n" + ex.Message);
            }
        }
    }

    /// <summary>
    /// Uninstall the application - removes startup registry, Start Menu shortcut, and application folder
    /// </summary>
    public static void Uninstall()
    {
        DialogResult result = MessageBox.Show(
            "Are you sure you want to uninstall LazyControl?\n\nThis will remove:\n" +
            "• Start Menu shortcut\n" +
            "• Windows startup registration\n" +
            "• All application files in AppData\n\n" +
            "The application will close after uninstallation.",
            "Confirm Uninstall",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.Yes)
        {
            try
            {
                // 1. Xóa registry startup
                RegisterInStartup(false);

                // 2. Xóa shortcut Start Menu
                RemoveStartMenuShortcut();

                // 3. Lấy đường dẫn thư mục AppData
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    AppName
                );

                // 4. Hiển thị thông báo thành công trước khi xóa file
                MessageBox.Show(
                    "Uninstallation completed successfully!\n\nThe application will close now.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // 5. Tạo batch file để xóa thư mục sau khi ứng dụng đóng
                CreateUninstallBatch(appDataPath);

                // 6. Thoát ứng dụng
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred during uninstallation:\n" + ex.Message +
                    "\n\nPlease try manually deleting the folder:\n" +
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName),
                    "Uninstallation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }

    /// <summary>
    /// Remove shortcut from Start Menu
    /// </summary>
    private static void RemoveStartMenuShortcut()
    {
        try
        {
            string shortcutPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "Programs",
                $"{AppName}.lnk"
            );

            if (System.IO.File.Exists(shortcutPath))
            {
                System.IO.File.Delete(shortcutPath);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't stop the uninstallation process
            Debug.WriteLine($"Could not remove Start Menu shortcut: {ex.Message}");
        }
    }

    /// <summary>
    /// Create batch file to delete application folder after exe has closed
    /// Since we can't delete a running file, use batch file to delete later
    /// </summary>
    private static void CreateUninstallBatch(string appDataPath)
    {
        try
        {
            string batchPath = Path.Combine(Path.GetTempPath(), "uninstall_lazycontrol.bat");

            string batchContent = $@"@echo off
:: Wait 2 seconds for the application to close completely
timeout /t 2 /nobreak >nul

:: Delete application folder
if exist ""{appDataPath}"" (
    rmdir /s /q ""{appDataPath}""
)

:: Delete the batch file itself
del ""%~f0""
";

            System.IO.File.WriteAllText(batchPath, batchContent);

            // Run batch file hidden
            Process.Start(new ProcessStartInfo
            {
                FileName = batchPath,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not create uninstall batch file: {ex.Message}");
            // Fallback: try to delete folder directly (may not succeed)
            try
            {
                if (Directory.Exists(appDataPath))
                {
                    Directory.Delete(appDataPath, true);
                }
            }
            catch
            {
                // Ignore - file is being used
            }
        }
    }
}
