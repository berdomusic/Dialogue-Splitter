using System.Diagnostics;

namespace VO_Tool.Services
{
    public class WhisperSegment
    {
        public double Start { get; set; }
        public double End { get; set; }
        public string Text { get; set; } = string.Empty;
        public string SourceAudioFile { get; set; } = string.Empty; 
    }
    
    public static class WhisperServiceHelper
    {
        public static string GetPythonCommand()
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
                        // Check if this Python has whisper installed
                        if (IsWhisperInstalled(cmd))
                        {
                            return cmd;
                        }
                    }
                }
                catch { }
            }
    
            return string.Empty;
        }
        
        public static bool IsWhisperInstalled(string pythonCmd)
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
        
        public static string GetWhisperScript(string audioFilePath, WhisperModel model, WhisperLanguage language, List<string> expectedTexts)
        {
            var modelName = model.ToModelString();
            var languageCode = language.ToLanguageCode();

            // Only add language parameter if not Auto
            var languageParam = language != WhisperLanguage.Auto ? $", language='{languageCode}'" : "";

            return @"
import whisper
import sys

audio_file = r'" + audioFilePath + @"'

model = whisper.load_model('" + modelName + @"')
result = model.transcribe(audio_file, word_timestamps=True" + languageParam + @")

for seg in result['segments']:
    start = seg['start']
    end = seg['end']
    text = seg['text'].strip()
    print(f'[{start:.2f}s - {end:.2f}s] {text}', flush=True)
";
        }
    }
}