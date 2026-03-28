using System.Diagnostics;
using System.Reflection;
using System.Text;

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
        private static string? _scriptContent;
        
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
                        return cmd;
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
        
        public static string GetEmbeddedScriptPath()
        {
            if (_scriptContent == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
        
                // Get all resource names for debugging
                var resourceNames = assembly.GetManifestResourceNames();
        
                // Find the Python script resource
                var resourceName = resourceNames.FirstOrDefault(r => r.EndsWith("whisper_transcribe.py"));
        
                if (resourceName == null)
                {
                    var availableResources = string.Join(", ", resourceNames);
                    throw new Exception($"Embedded script not found. Available resources: {availableResources}");
                }
        
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new Exception("Could not load embedded script: " + resourceName);
            
                    using (var reader = new StreamReader(stream))
                    {
                        _scriptContent = reader.ReadToEnd();
                    }
                }
            }
    
            var scriptPath = Path.GetTempFileName() + ".py";
            File.WriteAllText(scriptPath, _scriptContent);
            return scriptPath;
        }
        
        public static async Task<WhisperResult> RunWhisperScript(
            string pythonCmd, 
            string scriptPath, 
            string audioFilePath, 
            string modelName, 
            string languageCode,
            Action<string>? onProgress = null)
        {
            var arguments = $"\"{scriptPath}\" \"{audioFilePath}\" \"{modelName}\" \"{languageCode}\"";
            
            var process = new Process();
            process.StartInfo.FileName = pythonCmd;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            
            var output = new StringBuilder();
            var error = new StringBuilder();
            
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
                    error.AppendLine(args.Data);
                    onProgress?.Invoke(args.Data);
                }
            };
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.WaitForExitAsync();
            
            return new WhisperResult
            {
                Output = output.ToString(),
                Error = error.ToString(),
                ExitCode = process.ExitCode
            };
        }
    }
    
    public class WhisperResult
    {
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }
    }
}