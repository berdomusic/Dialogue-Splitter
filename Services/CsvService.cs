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
            
            // Write header
            sb.AppendLine("Start (s),End (s),Duration (s),Transcribed Text,Expected Text,Audio File Name,Similarity,Source Audio File");
            
            foreach (var match in validMatches)
            {
                var segment = match.Segment!;
                double duration = segment.End - segment.Start;
                
                // Escape quotes in text fields
                string transcribed = segment.Text.Replace("\"", "\"\"");
                string expected = match.ExpectedText.Replace("\"", "\"\"");
                string audioFileName = match.AudioFileName.Replace("\"", "\"\"");
                string sourceAudioFile = Path.GetFileName(segment.SourceAudioFile).Replace("\"", "\"\"");
                
                sb.AppendLine($"{segment.Start:F2},{segment.End:F2},{duration:F2},\"{transcribed}\",\"{expected}\",\"{audioFileName}\",{match.Similarity:F4},\"{sourceAudioFile}\"");
            }
            
            return sb.ToString();
        }
        
        // Save CSV data to file in the same folder structure as logs
        public static void SaveCsvData(string csvData, string outputFolder, string modelName, string languageName, string timestamp = null)
        {
            if (string.IsNullOrEmpty(csvData))
                return;
            
            timestamp ??= DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var folderName = $"log_{timestamp}_{modelName}_{languageName}";
            var logFolder = Path.Combine(outputFolder, folderName);
            
            // Create the folder if it doesn't exist
            Directory.CreateDirectory(logFolder);
            
            // Put CSV file inside the same folder as the log
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
            sb.AppendLine("Start (s),End (s),Duration (s),Transcribed Text,Expected Text,Audio File Name,Similarity,Is Match,Source Audio File");
            
            foreach (var match in matches)
            {
                if (match.Segment != null)
                {
                    double duration = match.Segment.End - match.Segment.Start;
                    
                    // Escape quotes in text fields
                    string transcribed = match.Segment.Text.Replace("\"", "\"\"");
                    string expected = match.ExpectedText.Replace("\"", "\"\"");
                    string audioFileName = match.AudioFileName.Replace("\"", "\"\"");
                    string sourceAudioFile = Path.GetFileName(match.Segment.SourceAudioFile).Replace("\"", "\"\"");
                    
                    sb.AppendLine($"{match.Segment.Start:F2},{match.Segment.End:F2},{duration:F2},\"{transcribed}\",\"{expected}\",\"{audioFileName}\",{match.Similarity:F4},{match.IsMatch},\"{sourceAudioFile}\"");
                }
                else
                {
                    string expected = match.ExpectedText.Replace("\"", "\"\"");
                    string audioFileName = match.AudioFileName.Replace("\"", "\"\"");
                    
                    sb.AppendLine($",,,,\"{expected}\",\"{audioFileName}\",{match.Similarity:F4},{match.IsMatch},");
                }
            }
            
            return sb.ToString();
        }
    }
}