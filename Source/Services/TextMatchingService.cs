using System.Text;

namespace VO_Tool.Services
{
    public class MatchResult
    {
        public WhisperSegment? Segment { get; init; }
        public string ExpectedText { get; init; } = string.Empty;
        public string AudioFileName { get; init; } = string.Empty;
        public double Similarity { get; init; }
        public bool IsMatch { get; init; }
    }

    public static class TextMatchingService
    {
        // Calculate similarity between transcribed text and expected text
        private static double CalculateSimilarity(string transcribed, string expected)
        {
            if (string.IsNullOrEmpty(transcribed) || string.IsNullOrEmpty(expected))
                return 0;
            
            transcribed = transcribed.ToLowerInvariant().Trim();
            expected = expected.ToLowerInvariant().Trim();
            
            // Exact match
            if (transcribed == expected)
                return 1.0;
            
            // Contains match
            if (transcribed.Contains(expected) || expected.Contains(transcribed))
                return 0.95;
            
            // Levenshtein ratio
            int distance = LevenshteinDistance(transcribed, expected);
            int maxLength = Math.Max(transcribed.Length, expected.Length);
            double levenshteinRatio = 1.0 - (double)distance / maxLength;
            
            // Word overlap ratio
            var transcribedWords = transcribed.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var expectedWords = expected.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            
            int commonWords = transcribedWords.Intersect(expectedWords).Count();
            double wordOverlap = commonWords / (double)Math.Max(transcribedWords.Length, expectedWords.Length);
            
            // Weighted combination (levenshtein: 70%, word overlap: 30%)
            return (levenshteinRatio * 0.7) + (wordOverlap * 0.3);
        }
        
        // Match all segments to texts - finds every segment that matches any text above threshold
        public static List<MatchResult> MatchSegmentsToTexts(
            List<WhisperSegment> segments, 
            List<string> texts, 
            List<string> audioFileNames,
            double threshold)
        {
            var results = new List<MatchResult>();
            
            if (segments.Count == 0 || texts.Count == 0)
                return results;
            
            // For each segment, find the best matching text
            foreach (var segment in segments)
            {
                double bestSimilarity = 0;
                int bestTextIndex = -1;
                
                // Check against all texts to find best match
                for (int i = 0; i < texts.Count; i++)
                {
                    double sim = CalculateSimilarity(segment.Text, texts[i]);
                    if (sim > bestSimilarity)
                    {
                        bestSimilarity = sim;
                        bestTextIndex = i;
                    }
                }
                
                if (bestSimilarity >= threshold && bestTextIndex >= 0)
                {
                    results.Add(new MatchResult
                    {
                        Segment = segment,
                        ExpectedText = texts[bestTextIndex],
                        AudioFileName = audioFileNames[bestTextIndex],
                        Similarity = bestSimilarity,
                        IsMatch = true
                    });
                }
                else
                {
                    results.Add(new MatchResult
                    {
                        Segment = segment,
                        ExpectedText = "UNMATCHED_SEGMENT",
                        AudioFileName = "",
                        Similarity = bestSimilarity,
                        IsMatch = false
                    });
                }
            }
            
            // Add any texts that had no matches at all (for reporting)
            var matchedTexts = results.Where(r => r.IsMatch).Select(r => r.ExpectedText).Distinct().ToHashSet();
            foreach (var text in texts)
            {
                if (!matchedTexts.Contains(text))
                {
                    int index = texts.IndexOf(text);
                    results.Add(new MatchResult
                    {
                        Segment = null,
                        ExpectedText = text,
                        AudioFileName = audioFileNames[index],
                        Similarity = 0,
                        IsMatch = false
                    });
                }
            }
            
            // Sort by segment start time (chronological order)
            return results.OrderBy(r => r.Segment?.Start ?? double.MaxValue).ToList();
        }
        
        // Save match results to CSV file
        public static void SaveMatchesToCsv(List<MatchResult> matches, string outputFolder)
        {
            var csvPath = Path.Combine(outputFolder, $"match_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            using var writer = new StreamWriter(csvPath, false, Encoding.UTF8);
            writer.WriteLine("Start (s),End (s),Transcribed Text,Expected Text,Audio File Name,Similarity,Is Match");
                
            foreach (var match in matches)
            {
                if (match.Segment != null)
                {
                    // Escape quotes in text fields
                    string transcribed = match.Segment.Text.Replace("\"", "\"\"");
                    string expected = match.ExpectedText.Replace("\"", "\"\"");
                    string audioFileName = match.AudioFileName.Replace("\"", "\"\"");
                        
                    writer.WriteLine($"{match.Segment.Start:F2},{match.Segment.End:F2},\"{transcribed}\",\"{expected}\",\"{audioFileName}\",{match.Similarity:F4},{match.IsMatch}");
                }
                else
                {
                    string expected = match.ExpectedText.Replace("\"", "\"\"");
                    string audioFileName = match.AudioFileName.Replace("\"", "\"\"");
                        
                    writer.WriteLine($",,\"NO_SEGMENT\",\"{expected}\",\"{audioFileName}\",{match.Similarity:F4},{match.IsMatch}");
                }
            }
        }
        
        // Levenshtein distance calculation
        private static int LevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];
            
            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;
            
            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;
            
            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    var cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            
            return matrix[s1.Length, s2.Length];
        }
    }
}