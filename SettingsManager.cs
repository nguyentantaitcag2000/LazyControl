using LazyControl.Models.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LazyControl
{
    public static class SettingsManager
    {
        
        private static readonly string SettingsFile = "settings.json";
        
        public static AppSettings LoadSettings()
        {
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
            return JsonSerializer.Deserialize<AppSettings>(json);
        }

        public static void SaveSettings(AppSettings settings)
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }

        public static ResultSettingCheck checkValidSettings(AppSettings currentSettings)
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
    }
    public class ResultSettingCheck
    {
        public bool valid { get; set; }
        public string message { get; set; }
    }

}
