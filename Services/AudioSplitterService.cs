using NAudio.Wave;
using System.Text;
using VO_Tool.Status;

namespace VO_Tool.Services
{
    public class AudioSplitterService
    {
        public async Task SplitAudioFromCsv(string csvPath, string sourceAudioPath, string outputFolder, StatusManager statusManager)
        {
            var segments = ParseCsvFile(csvPath);
            
            if (segments.Count == 0)
            {
                statusManager.UpdateStatus("No segments to split");
                return;
            }
            
            statusManager.UpdateStatus($"Found {segments.Count} segments to split");
            
            // Create Media folder inside output folder
            var mediaFolder = Path.Combine(outputFolder, "Media");
            Directory.CreateDirectory(mediaFolder);
            
            // Track duplicate filenames to add indices
            var fileCounter = new Dictionary<string, int>();
            
            using (var reader = new AudioFileReader(sourceAudioPath))
            {
                int splitCount = 0;
                foreach (var segment in segments)
                {
                    // Generate unique filename for duplicates
                    string baseFileName = segment.OutputFileName;
                    string fileName = baseFileName;
                    
                    if (fileCounter.ContainsKey(baseFileName))
                    {
                        fileCounter[baseFileName]++;
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(baseFileName);
                        string ext = Path.GetExtension(baseFileName);
                        fileName = $"{nameWithoutExt}_{fileCounter[baseFileName]}{ext}";
                    }
                    else
                    {
                        fileCounter[baseFileName] = 0;
                    }
                    
                    segment.OutputFileName = fileName;
                    
                    statusManager.UpdateStatus($"Splitting: {baseFileName} -> {fileName} [{segment.StartTime:F1}s - {segment.EndTime:F1}s]", false);
                    
                    await SplitSegmentAsync(reader, segment, mediaFolder);
                    splitCount++;
                }
                
                statusManager.UpdateStatus($"Split complete: {splitCount} files created in Media folder");
            }
        }
        
        private List<AudioSegment> ParseCsvFile(string csvPath)
        {
            var segments = new List<AudioSegment>();
            var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
            
            // Skip header
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                    
                var parts = ParseCsvLine(line);
                if (parts.Length >= 4)
                {
                    var segment = new AudioSegment
                    {
                        StartTime = ParseTime(parts[1]),  // Start column
                        EndTime = ParseTime(parts[2]),    // End column
                        OutputFileName = parts[3].Trim('"'), // Audio File Name column
                        SourceAudioFile = parts[0].Trim('"')  // Source Audio File column
                    };
                    
                    // Ensure filename has .wav extension
                    if (!segment.OutputFileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                        segment.OutputFileName += ".wav";
                    
                    // Remove any invalid characters from filename
                    var invalidChars = Path.GetInvalidFileNameChars();
                    segment.OutputFileName = string.Concat(segment.OutputFileName.Select(c => invalidChars.Contains(c) ? '_' : c));
                    
                    if (segment.StartTime >= 0 && segment.EndTime > segment.StartTime)
                    {
                        segments.Add(segment);
                    }
                }
            }
            
            return segments;
        }
        
        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;
            
            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            
            return result.ToArray();
        }
        
        private double ParseTime(string timeStr)
        {
            timeStr = timeStr.Trim();
            var parts = timeStr.Split(':');
            
            if (parts.Length == 2)
            {
                int minutes = int.Parse(parts[0]);
                double seconds = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                return minutes * 60 + seconds;
            }
            else if (parts.Length == 3)
            {
                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                double seconds = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                return hours * 3600 + minutes * 60 + seconds;
            }
            
            return 0;
        }
        
        private async Task SplitSegmentAsync(AudioFileReader reader, AudioSegment segment, string mediaFolder)
        {
            var outputPath = Path.Combine(mediaFolder, segment.OutputFileName);
            
            // Calculate position in bytes
            int bytesPerSecond = reader.WaveFormat.AverageBytesPerSecond;
            long startByte = (long)(segment.StartTime * bytesPerSecond);
            long endByte = (long)(segment.EndTime * bytesPerSecond);
            int bytesToRead = (int)(endByte - startByte);
            
            // Seek to start position
            reader.Position = startByte;
            
            // Read the segment
            byte[] buffer = new byte[bytesToRead];
            int bytesRead = await reader.ReadAsync(buffer, 0, bytesToRead);
            
            if (bytesRead > 0)
            {
                using (var writer = new WaveFileWriter(outputPath, reader.WaveFormat))
                {
                    await writer.WriteAsync(buffer, 0, bytesRead);
                }
            }
        }
    }
    
    public class AudioSegment
    {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public string OutputFileName { get; set; } = string.Empty;
        public string SourceAudioFile { get; set; } = string.Empty;
    }
}