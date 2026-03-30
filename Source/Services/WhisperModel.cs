namespace VO_Tool.Services
{
    public enum WhisperModel
    {
        Tiny,
        Base,
        Small,
        Medium,
        Large,
        TinyEn,
        BaseEn,
        SmallEn,
        MediumEn,
        LargeV1,
        LargeV2,
        LargeV3,
        LargeV3Turbo,
        Turbo
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
            { "large-v2", WhisperModel.LargeV2 },
            { "large-v3", WhisperModel.LargeV3 },
            { "tiny.en", WhisperModel.TinyEn },
            { "base.en", WhisperModel.BaseEn },
            { "small.en", WhisperModel.SmallEn },
            { "medium.en", WhisperModel.MediumEn },
            { "large-v1", WhisperModel.LargeV1 },
            { "large-v3-turbo", WhisperModel.LargeV3Turbo },
            { "turbo", WhisperModel.Turbo }
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
                WhisperModel.TinyEn => "tiny.en",
                WhisperModel.BaseEn => "base.en",
                WhisperModel.SmallEn => "small.en",
                WhisperModel.MediumEn => "medium.en",
                WhisperModel.LargeV1 => "large-v1",
                WhisperModel.LargeV2 => "large-v2",
                WhisperModel.LargeV3 => "large-v3",
                WhisperModel.LargeV3Turbo => "large-v3-turbo",
                WhisperModel.Turbo => "turbo",
                _ => "base"
            };
        }
        
        public static WhisperModel FromString(string modelName)
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