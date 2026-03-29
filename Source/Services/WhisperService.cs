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
    
    var modelName = model.ToModelString();
    var languageCode = language == WhisperLanguage.Auto ? "auto" : language.ToLanguageCode();
    
    var scriptPath = WhisperServiceHelper.GetEmbeddedScriptPath();
    
    try
    {
        var result = await WhisperServiceHelper.RunWhisperScript(
            pythonCmd, scriptPath, audioFilePath, modelName, languageCode, onProgress);
        
        if (result.ExitCode != 0)
        {
            throw new Exception($"Whisper failed with exit code {result.ExitCode}\nError: {result.Error}");
        }
        
        // Parse segments from output
        var lines = result.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("[") && line.Contains(" - "))
            {
                var parts = line.Split(']')[0].TrimStart('[').Split(" - ");
                if (parts.Length == 2)
                {
                    if (TryParseFormattedTime(parts[0], out double start) &&
                        TryParseFormattedTime(parts[1], out double end))
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

private static bool TryParseFormattedTime(string timeStr, out double seconds)
{
    seconds = 0;
    try
    {
        var parts = timeStr.Split(':');
        if (parts.Length == 2)
        {
            int minutes = int.Parse(parts[0]);
            double secs = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
            seconds = minutes * 60 + secs;
            return true;
        }
        else if (parts.Length == 3)
        {
            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            double secs = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
            seconds = hours * 3600 + minutes * 60 + secs;
            return true;
        }
    }
    catch { }
    return false;
}
    }
}