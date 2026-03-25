using System.Text.Json;

namespace VO_Tool.Settings
{
    public class AppSettings
    {
        public string LastExcelFile { get; set; } = string.Empty;
        public string LastAudioFile { get; set; } = string.Empty;
        public string LastOutputFolder { get; set; } = string.Empty;
        public string LastVO_Text_Column { get; set; } = string.Empty;
        public string LastVO_Audio_Column { get; set; } = string.Empty;
        public int LastSimilarityThreshold { get; set; } = 75;
    }
    
    public static class Settings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VO_Tool", "settings.json"
        );
        
        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { }
            
            return new AppSettings();
        }
        
        public static void Save(AppSettings settings)
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }
    }
}