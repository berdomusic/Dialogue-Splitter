using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VO_Tool.Services
{
    public class ReaperService
    {
        public async Task<List<string>> GetTrackNamesAsync(string reaperPath)
        {
            var content = await File.ReadAllTextAsync(reaperPath);
            var tracks = new List<string>();
            
            var trackPattern = @"<TRACK\s+([\s\S]*?)>";
            var namePattern = @"NAME\s+""([^""]+)""";
            
            var trackMatches = Regex.Matches(content, trackPattern);
            var trackNumber = 1;
            
            foreach (Match trackMatch in trackMatches)
            {
                var trackContent = trackMatch.Groups[1].Value;
                var nameMatch = Regex.Match(trackContent, namePattern);
                
                if (nameMatch.Success)
                {
                    string trackName = nameMatch.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(trackName))
                    {
                        tracks.Add(trackName);
                    }
                    else
                    {
                        tracks.Add($"Track {trackNumber}");
                    }
                }
                else
                {
                    tracks.Add($"Track {trackNumber}");
                }
                trackNumber++;
            }
            
            return tracks;
        }
    }
}