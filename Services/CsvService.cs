using System.Text;

namespace VO_Tool.Services
{
    public static class CsvService
    {
        // Create CSV data in memory as string
        public static string CreateMatchesCsvData(List<MatchResult> matches)
        {
            // Filter only matches that passed the similarity threshold
            var validMatches = matches.Where(m => m.IsMatch && m.Segment != null).ToList();
            
            if (validMatches.Count == 0)
                return string.Empty;
            
            var sb = new StringBuilder();
            
            // Write header with new order
            sb.AppendLine("Source Audio File,Start,End,Audio File Name,Expected Text,Transcribed Text,Similarity");
            
            foreach (var match in validMatches)
            {
                var segment = match.Segment!;
                
                // Format times with milliseconds
                string startTime = FormatTime(segment.Start);
                string endTime = FormatTime(segment.End);
                
                // Escape quotes in text fields
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
        
        // Save CSV data to file
        public static void SaveCsvData(string csvData, string outputFolder, string modelName, string languageName, string timestamp = null)
        {
            if (string.IsNullOrEmpty(csvData))
                return;
            
            timestamp ??= DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var folderName = $"log_{timestamp}_{modelName}_{languageName}";
            var logFolder = Path.Combine(outputFolder, folderName);
            
            // Create the folder if it doesn't exist
            Directory.CreateDirectory(logFolder);
            
            // Put CSV file inside the folder
            var csvPath = Path.Combine(logFolder, $"matches_{timestamp}.csv");
            
            File.WriteAllText(csvPath, csvData, Encoding.UTF8);
        }
        
        // Overload for when you have the log folder already created
        public static void SaveCsvData(string csvData, string logFolder, string fileName = null)
        {
            if (string.IsNullOrEmpty(csvData))
                return;
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var csvPath = Path.Combine(logFolder, fileName ?? $"matches_{timestamp}.csv");
            
            File.WriteAllText(csvPath, csvData, Encoding.UTF8);
        }
        
        // Create all matches CSV data (including unmatched)
        public static string CreateAllMatchesCsvData(List<MatchResult> matches)
        {
            var sb = new StringBuilder();
            
            // Write header
            sb.AppendLine("Source Audio File,Start,End,Audio File Name,Expected Text,Transcribed Text,Similarity,Is Match");
            
            foreach (var match in matches)
            {
                if (match.Segment != null)
                {
                    // Format times with milliseconds
                    string startTime = FormatTime(match.Segment.Start);
                    string endTime = FormatTime(match.Segment.End);
                    
                    // Escape quotes in text fields
                    string sourceAudioFile = Path.GetFileName(match.Segment.SourceAudioFile).Replace("\"", "\"\"");
                    string audioFileName = match.AudioFileName.Replace("\"", "\"\"");
                    string expected = match.ExpectedText.Replace("\"", "\"\"");
                    string transcribed = match.Segment.Text.Replace("\"", "\"\"");
                    
                    sb.AppendLine($"\"{sourceAudioFile}\",{startTime},{endTime},\"{audioFileName}\",\"{expected}\",\"{transcribed}\",{match.Similarity:F4},{match.IsMatch}");
                }
                else
                {
                    string expected = match.ExpectedText.Replace("\"", "\"\"");
                    string audioFileName = match.AudioFileName.Replace("\"", "\"\"");
                    
                    sb.AppendLine($",,,,\"{expected}\",\"{audioFileName}\",{match.Similarity:F4},{match.IsMatch}");
                }
            }
            
            return sb.ToString();
        }
    }
}