namespace VO_Tool.Services
{
    public enum WhisperModel
    {
        Tiny,
        Base,
        Small,
        Medium,
        Large
    }
    
    public static class WhisperModelExtensions
    {
        private static readonly Dictionary<string, WhisperModel> ModelNameMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "tiny", WhisperModel.Tiny },
            { "base", WhisperModel.Base },
            { "small", WhisperModel.Small },
            { "medium", WhisperModel.Medium },
            { "large", WhisperModel.Large },
            { "large-v2", WhisperModel.Large },
            { "large-v3", WhisperModel.Large }
        };
        
        public static string ToModelString(this WhisperModel model)
        {
            return model switch
            {
                WhisperModel.Tiny => "tiny",
                WhisperModel.Base => "base",
                WhisperModel.Small => "small",
                WhisperModel.Medium => "medium",
                WhisperModel.Large => "large",
                _ => "base"
            };
        }

        private static WhisperModel FromString(string modelName)
        {
            if (string.IsNullOrEmpty(modelName)) return WhisperModel.Base;
            return ModelNameMap.GetValueOrDefault(modelName, WhisperModel.Base);
        }
        
        public static List<WhisperModel> GetInstalledModels()
        {
            var installed = new HashSet<WhisperModel>();
            var cachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".cache", "whisper"
            );
            
            if (Directory.Exists(cachePath))
            {
                foreach (var file in Directory.GetFiles(cachePath, "*.pt"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    
                    // Skip .en models
                    if (fileName.EndsWith(".en", StringComparison.OrdinalIgnoreCase))
                        continue;
                    
                    var model = FromString(fileName);
                    installed.Add(model);
                }
            }
            
            // Always include base as fallback
            if (installed.Count == 0)
            {
                installed.Add(WhisperModel.Base);
            }
            
            return installed.OrderBy(m => m).ToList();
        }
    }
}