using Microsoft.Win32;
using System.Diagnostics;
using IWshRuntimeLibrary; // Thư viện này cài vào bằng cách chuột phải vào Dependencies và chọn Add Project Reference, sau đó > COM sau đó search "Windows Script Host Object Model", lúc này tick vào tuỳ chọn "IWshRuntimeLibrary" để cài thư viện

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
                var shell = new WshShell();
                var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = Application.ExecutablePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                shortcut.WindowStyle = 1;
                shortcut.Description = "Ứng dụng điều khiển chuột bằng bàn phím LazyControl";
                shortcut.IconLocation = Application.ExecutablePath;
                shortcut.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tạo shortcut Start Menu:\n" + ex.Message);
            }
        }
    }
}
