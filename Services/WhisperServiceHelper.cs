namespace VO_Tool.Services
{
    public static class WhisperServiceHelper
    {
        public static string GetWhisperScript(string audioFilePath)
        {
            return @"
import whisper
import sys

audio_file = r'" + audioFilePath + @"'

model = whisper.load_model('base')
result = model.transcribe(audio_file, word_timestamps=True)

for seg in result['segments']:
    start = seg['start']
    end = seg['end']
    text = seg['text'].strip()
    print(f'[{start:.2f}s - {end:.2f}s] {text}', flush=True)
";
        }
    }
}