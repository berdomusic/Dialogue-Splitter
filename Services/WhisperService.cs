using System.Diagnostics;
using System.Text;
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
        
        public static async Task<List<WhisperSegment>> TranscribeAsync(string audioFilePath)
        {
            var segments = new List<WhisperSegment>();
            
            string pythonCmd = GetPythonCommand();
            if (string.IsNullOrEmpty(pythonCmd))
            {
                throw new Exception(
                    "Python is not installed or not in PATH.\n\n" +
                    "Please install Python 3.8 or later from:\n" +
                    "https://www.python.org/downloads/\n\n" +
                    "Make sure to check 'Add Python to PATH' during installation."
                );
            }
            
            if (!IsWhisperInstalled(pythonCmd))
            {
                throw new Exception(
                    $"Whisper is not installed.\n\n" +
                    "Please open Command Prompt and run:\n" +
                    $"{pythonCmd} -m pip install openai-whisper\n\n" +
                    "This may take a few minutes to download."
                );
            }
            
            var scriptPath = Path.GetTempFileName() + ".py";
            var outputPath = Path.GetTempFileName() + ".json";
            
            var script = @"
import whisper
import json

model = whisper.load_model('base')
result = model.transcribe('" + audioFilePath.Replace("\\", "\\\\") + @"', word_timestamps=True)

segments = []
for seg in result['segments']:
    segments.append({
        'start': seg['start'],
        'end': seg['end'],
        'text': seg['text'].strip()
    })

with open(r'" + outputPath + @"', 'w', encoding='utf-8') as f:
    json.dump(segments, f, ensure_ascii=False)
";
            
            await File.WriteAllTextAsync(scriptPath, script);
            
            try
            {
                var process = new Process();
                process.StartInfo.FileName = pythonCmd;
                process.StartInfo.Arguments = $"\"{scriptPath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                
                var error = new StringBuilder();
                
                process.ErrorDataReceived += (sender, args) => { if (args.Data != null) error.AppendLine(args.Data); };
                
                process.Start();
                process.BeginErrorReadLine();
                
                await process.WaitForExitAsync();
                
                if (process.ExitCode != 0)
                {
                    throw new Exception($"Whisper failed:\n{error}");
                }
                
                if (File.Exists(outputPath))
                {
                    var json = await File.ReadAllTextAsync(outputPath);
                    segments = JsonSerializer.Deserialize<List<WhisperSegment>>(json) ?? new();
                }
            }
            finally
            {
                try { File.Delete(scriptPath); } catch { }
                try { File.Delete(outputPath); } catch { }
            }
            
            return segments;
        }
        
        public static double CalculateSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0;
            
            text1 = text1.ToLowerInvariant().Trim();
            text2 = text2.ToLowerInvariant().Trim();
            
            if (text1 == text2)
                return 1.0;
            
            if (text1.Contains(text2) || text2.Contains(text1))
                return 0.9;
            
            var distance = LevenshteinDistance(text1, text2);
            var maxLength = Math.Max(text1.Length, text2.Length);
            
            return 1.0 - (double)distance / maxLength;
        }
        
        private static int LevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];
            
            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;
            
            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;
            
            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    var cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            
            return matrix[s1.Length, s2.Length];
        }
        
        private static string GetPythonCommand()
        {
            string[] possibleCommands = { "py", "python", "python3" };
            
            foreach (var cmd in possibleCommands)
            {
                try
                {
                    var process = new Process();
                    process.StartInfo.FileName = cmd;
                    process.StartInfo.Arguments = "--version";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    
                    process.Start();
                    process.WaitForExit(3000);
                    
                    if (process.ExitCode == 0)
                    {
                        return cmd;
                    }
                }
                catch { }
            }
            
            return string.Empty;
        }
        
        private static bool IsWhisperInstalled(string pythonCmd)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = pythonCmd;
                process.StartInfo.Arguments = "-c \"import whisper; print('OK')\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(3000);
                
                return process.ExitCode == 0 && output.Contains("OK");
            }
            catch
            {
                return false;
            }
        }
    }
}