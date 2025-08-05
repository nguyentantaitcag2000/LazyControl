using LazyControl.Models.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LazyControl
{
    public static class SettingsManager
    {
        // Sử dụng thư mục của ứng dụng thay vì đường dẫn tương đối
        private static readonly string SettingsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LazyControl",
            "settings.json"
        );

        public static AppSettings LoadSettings()
        {
            try
            {
                // Đảm bảo thư mục tồn tại (nếu sử dụng AppData)
                string directory = Path.GetDirectoryName(SettingsFile);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(SettingsFile))
                {
                    // Tạo mặc định và lưu vào file
                    var defaultSettings = new AppSettings
                    {
                        EscF1 = 1,
                        EscF2 = 2
                    };

                    SaveSettings(defaultSettings);
                    return defaultSettings;
                }

                string json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings
                {
                    EscF1 = 1,
                    EscF2 = 2
                };
            }
            catch (Exception ex)
            {
                // Log lỗi hoặc hiển thị thông báo
                Debug.WriteLine($"Error loading settings: {ex.Message}");

                // Trả về settings mặc định nếu có lỗi
                return new AppSettings
                {
                    EscF1 = 1,
                    EscF2 = 2
                };
            }
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                // Đảm bảo thư mục tồn tại
                string directory = Path.GetDirectoryName(SettingsFile);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                // Log lỗi hoặc hiển thị thông báo cho user
                Debug.WriteLine($"Error saving settings: {ex.Message}");

                // Có thể hiển thị MessageBox thông báo lỗi cho user
                System.Windows.Forms.MessageBox.Show(
                    $"Cannot save settings: {ex.Message}\nSettings path: {SettingsFile}",
                    "Settings Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning
                );
            }
        }

        public static ResultSettingCheck checkValidSettings(AppSettings currentSettings)
        {
            try
            {
                // Không cho phép trùng nhau với các giá trị screen options
                if (currentSettings.EscF1 == currentSettings.EscF2)
                {
                    return new ResultSettingCheck
                    {
                        valid = false,
                        message = "ESC F1 & ESC F2 is same screen"
                    };
                }
                return new ResultSettingCheck
                {
                    valid = true,
                };
            }
            catch (Exception ex)
            {
                return new ResultSettingCheck
                {
                    valid = false,
                    message = $"Error validating settings: {ex.Message}"
                };
            }
        }

        // Thêm phương thức để lấy đường dẫn settings file (để debug)
        public static string GetSettingsPath()
        {
            return SettingsFile;
        }
    }

    public class ResultSettingCheck
    {
        public bool valid { get; set; }
        public string message { get; set; } = string.Empty;
    }
}