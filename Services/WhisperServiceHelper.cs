namespace VO_Tool.Services
{
    public static class WhisperServiceHelper
    {
        public static string GetWhisperScript(string audioFilePath, string modelName)
        {
            return @"
import whisper
import sys

audio_file = r'" + audioFilePath + @"'

model = whisper.load_model('" + modelName + @"')
result = model.transcribe(audio_file, word_timestamps=True)

for seg in result['segments']:
    start = seg['start']
    end = seg['end']
    text = seg['text'].strip()
    print(f'[{start:.2f}s - {end:.2f}s] {text}', flush=True)
";
        }
        
        public static List<string> GetInstalledModels()
        {
            var installed = new List<string>();
            var cachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".cache", "whisper"
            );
            
            if (Directory.Exists(cachePath))
            {
                foreach (var file in Directory.GetFiles(cachePath, "*.pt"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName == "tiny" || fileName == "tiny.en") 
                        installed.Add("tiny");
                    else if (fileName == "base" || fileName == "base.en") 
                        installed.Add("base");
                    else if (fileName == "small" || fileName == "small.en") 
                        installed.Add("small");
                    else if (fileName == "medium" || fileName == "medium.en") 
                        installed.Add("medium");
                    else if (fileName == "large" || fileName == "large-v2" || fileName == "large-v3") 
                        installed.Add("large");
                }
            }
            
            // Always include base as fallback (will be downloaded if needed)
            if (installed.Count == 0)
            {
                installed.Add("base");
            }
            
            return installed;
        }
    }
}