using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VO_Tool.Services
{
    public class ReaperProjectInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string File { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public List<string> Tracks { get; set; } = new();
    }
    
    public class ReaperService
    {
        private string? _scriptsFolder;
        
        public ReaperService()
        {
            InitializeScriptsFolder();
        }
        
        private void InitializeScriptsFolder()
        {
            // Create a folder in AppData for our Lua scripts
            _scriptsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VO_Tool", "LuaScripts"
            );
            
            if (!Directory.Exists(_scriptsFolder))
            {
                Directory.CreateDirectory(_scriptsFolder);
            }
            
            // Extract all embedded Lua scripts
            ExtractAllScripts();
        }
        
        private void ExtractAllScripts()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            
            foreach (var resourceName in resourceNames)
            {
                // Check if this is a Lua script from our LuaScripts folder
                if (resourceName.Contains(".LuaScripts.") && resourceName.EndsWith(".lua"))
                {
                    // Extract the filename from the resource name
                    var fileName = resourceName.Split('.').Last();
                    var scriptPath = Path.Combine(_scriptsFolder, fileName);
                    
                    // Extract the script
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        var content = reader.ReadToEnd();
                        File.WriteAllText(scriptPath, content);
                    }
                }
            }
        }
        
        public async Task<List<ReaperProjectInfo>> GetOpenProjectsAsync()
        {
            var scriptPath = Path.Combine(_scriptsFolder, "GetOpenProjects.lua");
            var reaperPath = FindReaperPath();
            
            if (string.IsNullOrEmpty(reaperPath))
            {
                throw new Exception("Reaper not found. Please make sure Reaper is installed.");
            }
            
            if (!File.Exists(scriptPath))
            {
                throw new Exception("Lua script not found. Please reinstall the application.");
            }
            
            try
            {
                // Run Reaper with script
                var process = new Process();
                process.StartInfo.FileName = reaperPath;
                process.StartInfo.Arguments = $"-script \"{scriptPath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                
                var output = new StringBuilder();
                var error = new StringBuilder();
                
                process.OutputDataReceived += (sender, args) => output.AppendLine(args.Data);
                process.ErrorDataReceived += (sender, args) => error.AppendLine(args.Data);
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                await process.WaitForExitAsync();
                
                // Parse output from console
                var outputText = output.ToString();
                var projects = ParseProjectsFromOutput(outputText);
                
                return projects;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get Reaper projects: {ex.Message}");
            }
        }
        
        private List<ReaperProjectInfo> ParseProjectsFromOutput(string output)
        {
            var projects = new List<ReaperProjectInfo>();
            
            // Find JSON between markers
            var startMarker = "REAPER_PROJECTS_START";
            var endMarker = "REAPER_PROJECTS_END";
            
            var startIndex = output.IndexOf(startMarker);
            var endIndex = output.IndexOf(endMarker);
            
            if (startIndex != -1 && endIndex != -1)
            {
                var jsonStart = startIndex + startMarker.Length;
                var jsonLength = endIndex - jsonStart;
                var json = output.Substring(jsonStart, jsonLength).Trim();
                
                try
                {
                    projects = JsonSerializer.Deserialize<List<ReaperProjectInfo>>(json) ?? new();
                }
                catch (JsonException)
                {
                    // JSON parsing failed - return empty list
                }
            }
            
            return projects;
        }
        
        private string? FindReaperPath()
        {
            // Common Reaper installation paths
            var possiblePaths = new[]
            {
                @"C:\Program Files\REAPER (x64)\reaper.exe",
                @"C:\Program Files\REAPER\reaper.exe",
                @"C:\Program Files (x86)\REAPER\reaper.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "REAPER (x64)", "reaper.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "REAPER", "reaper.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "REAPER", "reaper.exe")
            };
            
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            // Try to find in registry
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\reaper.exe");
                if (key != null)
                {
                    var path = key.GetValue("")?.ToString();
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            catch
            {
                // Registry access failed
            }
            
            return null;
        }
        
        public async Task<List<string>> GetTrackNamesAsync(string reaperPath)
        {
            var content = await File.ReadAllTextAsync(reaperPath);
            var tracks = new List<string>();
            
            var trackPattern = @"<TRACK\s+([\s\S]*?)>";
            var namePattern = @"NAME\s+""([^""]+)""";
            
            var trackMatches = Regex.Matches(content, trackPattern);
            var trackNumber = 1;
            
            foreach (Match trackMatch in trackMatches)
            {
                var trackContent = trackMatch.Groups[1].Value;
                var nameMatch = Regex.Match(trackContent, namePattern);
                
                if (nameMatch.Success)
                {
                    string trackName = nameMatch.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(trackName))
                    {
                        tracks.Add(trackName);
                    }
                    else
                    {
                        tracks.Add($"Track {trackNumber}");
                    }
                }
                else
                {
                    tracks.Add($"Track {trackNumber}");
                }
                trackNumber++;
            }
            
            return tracks;
        }
    }
}