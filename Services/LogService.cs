namespace VO_Tool.Services
{
    public static class LogService
    {
        private static List<string> _logMessages = new List<string>();
        
        public static void AddMessage(string message)
        {
            _logMessages.Add($"{DateTime.Now:HH:mm:ss.fff} - {message}");
        }
        
        public static void ClearMessages()
        {
            _logMessages.Clear();
        }
        
        public static void SaveLogToFile(string outputFolder, string audioFile, string excelFile, string textColumn, string audioColumn, WhisperModel model, WhisperLanguage language)
        {
            var modelName = model.ToModelString();
            var languageName = language == WhisperLanguage.Auto ? "Auto" : language.ToString();
            var logPath = Path.Combine(outputFolder, $"split_log_{DateTime.Now:yyyyMMdd_HHmmss}_{modelName}_{languageName}.txt");
            
            using (var writer = new StreamWriter(logPath))
            {
                writer.WriteLine("=== VO Audio Splitter Log ===");
                writer.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"Audio file: {audioFile}");
                writer.WriteLine($"Excel file: {excelFile}");
                writer.WriteLine($"Text column: {textColumn}");
                writer.WriteLine($"Audio file name column: {audioColumn}");
                writer.WriteLine($"Whisper model: {modelName}");
                writer.WriteLine($"Language: {languageName}");
                writer.WriteLine();
                
                writer.WriteLine("=== All Status Messages ===");
                foreach (var msg in _logMessages)
                {
                    writer.WriteLine(msg);
                }
                
                writer.WriteLine();
                writer.WriteLine("=== End of Log ===");
            }
            
            // Clear messages after saving to prevent them from appearing in future logs
            ClearMessages();
        }
    }
}