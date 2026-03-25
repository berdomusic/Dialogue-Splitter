using System.Diagnostics;
using System.Text.Json;

namespace VO_Tool.Services
{
    public class WhisperSegment
    {
        public double Start { get; set; }
        public double End { get; set; }
        public string Text { get; set; } = string.Empty;
    }
    
    public static class WhisperService
    {
        public static double GetSimilarityThreshold(TrackBar trackBar)
        {
            return trackBar.Value / 100.0;
        }
        
        public static async Task<List<WhisperSegment>> TranscribeAsync(string audioFilePath, double similarityThreshold)
        {
            var segments = new List<WhisperSegment>();
            
            // Check if Python is available
            if (!IsPythonAvailable())
            {
                throw new Exception("Python is not installed. Please install Python 3.8 or later.");
            }
            
            // Check if Whisper is installed
            if (!IsWhisperInstalled())
            {
                throw new Exception("Whisper is not installed. Run: pip install openai-whisper");
            }
            
            // Create temporary Python script
            var scriptPath = Path.GetTempFileName() + ".py";
            var outputPath = Path.GetTempFileName() + ".json";
            
            var script = $@"
import whisper
import json

model = whisper.load_model('base')
result = model.transcribe(
    r'{audioFilePath}',
    word_timestamps=True,
    similarity_threshold={similarityThreshold:F2}
)

segments = []
for seg in result['segments']:
    segments.append({{
        'start': seg['start'],
        'end': seg['end'],
        'text': seg['text'].strip()
    }})

with open(r'{outputPath}', 'w', encoding='utf-8') as f:
    json.dump(segments, f, ensure_ascii=False)
";
            
            await File.WriteAllTextAsync(scriptPath, script);
            
            try
            {
                // Run Python script
                var process = new Process();
                process.StartInfo.FileName = "python";
                process.StartInfo.Arguments = $"\"{scriptPath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                
                process.Start();
                await process.WaitForExitAsync();
                
                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    throw new Exception($"Whisper failed: {error}");
                }
                
                // Read results
                if (File.Exists(outputPath))
                {
                    var json = await File.ReadAllTextAsync(outputPath);
                    segments = JsonSerializer.Deserialize<List<WhisperSegment>>(json) ?? new();
                }
            }
            finally
            {
                // Clean up temp files
                try { File.Delete(scriptPath); } catch { }
                try { File.Delete(outputPath); } catch { }
            }
            
            return segments;
        }
        
        private static bool IsPythonAvailable()
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = "python";
                process.StartInfo.Arguments = "--version";
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
        
        private static bool IsWhisperInstalled()
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = "python";
                process.StartInfo.Arguments = "-c \"import whisper\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
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
    }
}