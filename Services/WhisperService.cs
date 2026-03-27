using System.Diagnostics;
using System.Text;

namespace VO_Tool.Services
{
    public static class WhisperService
    {
        public static double GetSimilarityThreshold(TrackBar trackBar)
        {
            return trackBar.Value / 100.0;
        }
        
        public static async Task<List<WhisperSegment>> TranscribeAsync(
            string audioFilePath, 
            WhisperModel model, 
            WhisperLanguage language,
            List<string> expectedTexts,
            Action<string>? onProgress = null)
        {
            var segments = new List<WhisperSegment>();
            
            string pythonCmd = WhisperServiceHelper.GetPythonCommand();
            if (string.IsNullOrEmpty(pythonCmd))
            {
                throw new Exception(
                    "Python is not installed or not in PATH.\n\n" +
                    "Please install Python 3.8 or later from:\n" +
                    "https://www.python.org/downloads/\n\n" +
                    "Make sure to check 'Add Python to PATH' during installation."
                );
            }
            
            if (!WhisperServiceHelper.IsWhisperInstalled(pythonCmd))
            {
                throw new Exception(
                    $"Whisper is not installed.\n\n" +
                    "Please open Command Prompt and run:\n" +
                    $"{pythonCmd} -m pip install openai-whisper\n\n" +
                    "This may take a few minutes to download."
                );
            }
            
            
            
            var scriptPath = Path.GetTempFileName() + ".py";
            var script = WhisperServiceHelper.GetWhisperScript(audioFilePath, model, language, expectedTexts);
            
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
                
                var output = new StringBuilder();
                
                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        output.AppendLine(args.Data);
                        onProgress?.Invoke(args.Data);
                    }
                };
                
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        onProgress?.Invoke(args.Data);
                    }
                };
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                await process.WaitForExitAsync();
                
                if (process.ExitCode != 0)
                {
                    throw new Exception($"Whisper failed with exit code {process.ExitCode}");
                }
                
                // Parse segments from output
                var lines = output.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("[") && line.Contains("s - "))
                    {
                        var parts = line.Split(']')[0].TrimStart('[').Split("s - ");
                        if (parts.Length == 2)
                        {
                            if (double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double start) &&
                                double.TryParse(parts[1].TrimEnd('s'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double end))
                            {
                                var text = line.Split(']')[1].Trim();
                                segments.Add(new WhisperSegment
                                {
                                    Start = start,
                                    End = end,
                                    Text = text,
                                    SourceAudioFile = audioFilePath
                                });
                            }
                        }
                    }
                }
            }
            finally
            {
                try { File.Delete(scriptPath); } catch { }
            }
            
            return segments;
        }
    }
}