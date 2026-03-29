using System.Text;

namespace VO_Tool.Services
{
    public static class CsvService
    {
        // Create CSV data in memory as string
        public static string CreateMatchesCsvData(List<MatchResult> matches, double startPadding, double endPadding)
        {
            var validMatches = matches.Where(m => m.IsMatch && m.Segment != null).ToList();
    
            if (validMatches.Count == 0)
                return string.Empty;
    
            var sb = new StringBuilder();
            sb.AppendLine("Source Audio File,Start,End,Audio File Name,Expected Text,Transcribed Text,Similarity");
    
            foreach (var match in validMatches)
            {
                var segment = match.Segment!;
        
                double adjustedStart = Math.Max(0, segment.Start - startPadding);
                double adjustedEnd = Math.Max(segment.End + endPadding, 0);
        
                string startTime = FormatTime(adjustedStart);
                string endTime = FormatTime(adjustedEnd);
        
                string sourceAudioFile = Path.GetFileName(segment.SourceAudioFile).Replace("\"", "\"\"");
                string audioFileName = match.AudioFileName.Replace("\"", "\"\"");
                string expected = match.ExpectedText.Replace("\"", "\"\"");
                string transcribed = segment.Text.Replace("\"", "\"\"");
        
                sb.AppendLine($"\"{sourceAudioFile}\",{startTime},{endTime},\"{audioFileName}\",\"{expected}\",\"{transcribed}\",{match.Similarity:F4}");
            }
    
            return sb.ToString();
        }
        
        // Format time in seconds to MM:SS.mmm or HH:MM:SS.mmm format
        private static string FormatTime(double seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            
            if (ts.TotalHours >= 1)
            {
                return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}";
            }
            else
            {
                return $"{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}";
            }
        }
    }
}