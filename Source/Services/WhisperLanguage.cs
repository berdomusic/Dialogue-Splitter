namespace VO_Tool.Services
{
    public enum WhisperLanguage
    {
        Auto,
        Afrikaans,
        Arabic,
        Armenian,
        Azerbaijani,
        Belarusian,
        Bosnian,
        Bulgarian,
        Catalan,
        Chinese,
        Croatian,
        Czech,
        Danish,
        Dutch,
        English,
        Estonian,
        Finnish,
        French,
        Galician,
        German,
        Greek,
        Hebrew,
        Hindi,
        Hungarian,
        Icelandic,
        Indonesian,
        Italian,
        Japanese,
        Kannada,
        Kazakh,
        Korean,
        Latvian,
        Lithuanian,
        Macedonian,
        Malay,
        Marathi,
        Maori,
        Nepali,
        Norwegian,
        Persian,
        Polish,
        Portuguese,
        Romanian,
        Russian,
        Serbian,
        Slovak,
        Slovenian,
        Spanish,
        Swahili,
        Swedish,
        Tagalog,
        Tamil,
        Thai,
        Turkish,
        Ukrainian,
        Urdu,
        Vietnamese,
        Welsh
    }
    
    public static class WhisperLanguageExtensions
    {
        private static readonly Dictionary<WhisperLanguage, string> LanguageCodeMap = new()
        {
            { WhisperLanguage.Auto, "auto" },
            { WhisperLanguage.Afrikaans, "af" },
            { WhisperLanguage.Arabic, "ar" },
            { WhisperLanguage.Armenian, "hy" },
            { WhisperLanguage.Azerbaijani, "az" },
            { WhisperLanguage.Belarusian, "be" },
            { WhisperLanguage.Bosnian, "bs" },
            { WhisperLanguage.Bulgarian, "bg" },
            { WhisperLanguage.Catalan, "ca" },
            { WhisperLanguage.Chinese, "zh" },
            { WhisperLanguage.Croatian, "hr" },
            { WhisperLanguage.Czech, "cs" },
            { WhisperLanguage.Danish, "da" },
            { WhisperLanguage.Dutch, "nl" },
            { WhisperLanguage.English, "en" },
            { WhisperLanguage.Estonian, "et" },
            { WhisperLanguage.Finnish, "fi" },
            { WhisperLanguage.French, "fr" },
            { WhisperLanguage.Galician, "gl" },
            { WhisperLanguage.German, "de" },
            { WhisperLanguage.Greek, "el" },
            { WhisperLanguage.Hebrew, "he" },
            { WhisperLanguage.Hindi, "hi" },
            { WhisperLanguage.Hungarian, "hu" },
            { WhisperLanguage.Icelandic, "is" },
            { WhisperLanguage.Indonesian, "id" },
            { WhisperLanguage.Italian, "it" },
            { WhisperLanguage.Japanese, "ja" },
            { WhisperLanguage.Kannada, "kn" },
            { WhisperLanguage.Kazakh, "kk" },
            { WhisperLanguage.Korean, "ko" },
            { WhisperLanguage.Latvian, "lv" },
            { WhisperLanguage.Lithuanian, "lt" },
            { WhisperLanguage.Macedonian, "mk" },
            { WhisperLanguage.Malay, "ms" },
            { WhisperLanguage.Marathi, "mr" },
            { WhisperLanguage.Maori, "mi" },
            { WhisperLanguage.Nepali, "ne" },
            { WhisperLanguage.Norwegian, "no" },
            { WhisperLanguage.Persian, "fa" },
            { WhisperLanguage.Polish, "pl" },
            { WhisperLanguage.Portuguese, "pt" },
            { WhisperLanguage.Romanian, "ro" },
            { WhisperLanguage.Russian, "ru" },
            { WhisperLanguage.Serbian, "sr" },
            { WhisperLanguage.Slovak, "sk" },
            { WhisperLanguage.Slovenian, "sl" },
            { WhisperLanguage.Spanish, "es" },
            { WhisperLanguage.Swahili, "sw" },
            { WhisperLanguage.Swedish, "sv" },
            { WhisperLanguage.Tagalog, "tl" },
            { WhisperLanguage.Tamil, "ta" },
            { WhisperLanguage.Thai, "th" },
            { WhisperLanguage.Turkish, "tr" },
            { WhisperLanguage.Ukrainian, "uk" },
            { WhisperLanguage.Urdu, "ur" },
            { WhisperLanguage.Vietnamese, "vi" },
            { WhisperLanguage.Welsh, "cy" }
        };
        
        public static string ToLanguageCode(this WhisperLanguage language)
        {
            return LanguageCodeMap.GetValueOrDefault(language, "auto");
        }
        
        public static WhisperLanguage FromLanguageCode(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
                return WhisperLanguage.Auto;
                
            foreach (var kvp in LanguageCodeMap)
            {
                if (kvp.Value.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;
            }
            
            return WhisperLanguage.Auto;
        }
        
        public static List<WhisperLanguage> GetSupportedLanguages()
        {
            return LanguageCodeMap.Keys
                .Where(l => l != WhisperLanguage.Auto)
                .OrderBy(l => l.ToString())
                .ToList();
        }
        
        public static string GetDisplayName(this WhisperLanguage language)
        {
            if (language == WhisperLanguage.Auto)
                return "Auto-detect";
            
            return language.ToString();
        }
    }
}