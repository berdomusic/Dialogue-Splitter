using System.Diagnostics;

namespace VO_Tool.UI
{
    public class FileSelectionHelper
    {
        private static readonly string[] SupportedFormats = { ".wav", ".mp3", ".flac", ".m4a", ".ogg", ".aac", ".opus", ".webm" };
        
        public AudioValidationResult ValidateAudioFile(string filePath)
        {
            var result = new AudioValidationResult();
            
            if (!File.Exists(filePath))
            {
                result.IsValid = false;
                result.ErrorMessage = "File does not exist.";
                return result;
            }
            
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            if (!SupportedFormats.Contains(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Unsupported format: {extension}. Supported formats: WAV, MP3, FLAC, M4A, OGG, AAC, OPUS, WEBM";
                return result;
            }
            
            result.Format = extension.TrimStart('.').ToUpperInvariant();
            
            var fileInfo = new FileInfo(filePath);
            result.FileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
            
            if (result.FileSizeMB > 500)
            {
                result.WarningMessage = $"File size is {result.FileSizeMB:F1} MB. Large files may take a long time to process.";
            }
            
            if (extension != ".wav")
            {
                result.NeedsFfmpeg = true;
                result.WarningMessage = (result.WarningMessage ?? "") + $" {result.Format} format requires FFmpeg for conversion.";
            }
            
            result.IsValid = true;
            return result;
        }
        
        public bool IsFfmpegAvailable()
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = "ffmpeg";
                process.StartInfo.Arguments = "-version";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit(3000);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
        
        public bool IsAudioFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return SupportedFormats.Contains(extension);
        }
    }
    
    public class AudioValidationResult
    {
        public bool IsValid { get; set; }
        public string Format { get; set; } = string.Empty;
        public double FileSizeMB { get; set; }
        public bool NeedsFfmpeg { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string WarningMessage { get; set; } = string.Empty;
        
        public string GetStatusMessage()
        {
            if (!IsValid)
            {
                return $"Error: {ErrorMessage}";
            }
            
            var message = $"{Format} format";
            
            if (FileSizeMB > 0)
            {
                message += $", {FileSizeMB:F1} MB";
            }
            
            if (!string.IsNullOrEmpty(WarningMessage))
            {
                message += $" - Warning: {WarningMessage}";
            }
            
            return message;
        }
    }
}