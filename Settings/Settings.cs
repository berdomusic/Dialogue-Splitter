using System.Text.Json;
using VO_Tool.Services;
using VO_Tool.UI;

namespace VO_Tool.Settings
{
    public class AppSettings
    {
        public string LastExcelFile { get; set; } = string.Empty;
        public string LastAudioFile { get; set; } = string.Empty;
        public string LastOutputFolder { get; set; } = string.Empty;
        public string LastVoTextColumn { get; set; } = string.Empty;
        public string LastVoAudioColumn { get; set; } = string.Empty;
        public int LastSimilarityThreshold { get; set; } = 75;
        public WhisperModel LastModel { get; set; } = WhisperModel.Base;
        public WhisperLanguage LastLanguage { get; set; } = WhisperLanguage.English;
        public bool CreateLogFile { get; set; } = true;
        public bool CreateCsvFile { get; set; } = true;
        public double StartPaddingSeconds { get; set; }
        public double EndPaddingSeconds { get; set; } = .2;
        public bool SplitAudio { get; set; }
        
        public void UpdateFromUi(UiControls ui)
        {
            LastExcelFile = ui.ExcelSelector.FilePath;
            LastAudioFile = ui.AudioSelector.FilePath;
            LastOutputFolder = ui.OutputFolderSelector.FolderPath;
            LastVoTextColumn = ui.CmbVoTextColumn.SelectedItem?.ToString() ?? string.Empty;
            LastVoAudioColumn = ui.CmbVoAudioColumn.SelectedItem?.ToString() ?? string.Empty;
            LastSimilarityThreshold = ui.TbSimilarityThreshold.Value;
            CreateLogFile = ui.ChkCreateLogFile.Checked;
            CreateCsvFile = ui.ChkCreateCsvFile.Checked;
            SplitAudio = ui.ChkSplitAudio.Checked;
            StartPaddingSeconds = (double)ui.NudStartPadding.Value;
            EndPaddingSeconds = (double)ui.NudEndPadding.Value;
            
            if (ui.CmbModel.SelectedItem is WhisperModel model)
            {
                LastModel = model;
            }
            
            if (ui.CmbLanguage.SelectedItem is WhisperLanguage language)
            {
                LastLanguage = language;
            }
        }
    }
    
    public static class Settings
    {
        private static AppSettings? _settings;
        private static readonly Lock Lock = new Lock();
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VO_Tool", "settings.json"
        );
        
        public static AppSettings Get()
        {
            if (_settings == null)
            {
                lock (Lock)
                {
                    if (_settings == null)
                    {
                        _settings = Load();
                    }
                }
            }
            return _settings;
        }
        
        private static AppSettings Load()
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
        
        public static void Save()
        {
            if (_settings == null) return;
            
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }
    }
}