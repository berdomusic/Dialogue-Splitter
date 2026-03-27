import whisper
import sys

def format_time(seconds):
    minutes = int(seconds // 60)
    secs = seconds % 60
    return f"{minutes:02d}:{secs:06.3f}"

audio_file = sys.argv[1]
model_name = sys.argv[2]
language = sys.argv[3] if len(sys.argv) > 3 and sys.argv[3] != "auto" else None

model = whisper.load_model(model_name)

if language:
    result = model.transcribe(audio_file, word_timestamps=True, language=language)
else:
    result = model.transcribe(audio_file, word_timestamps=True)

for seg in result['segments']:
    start = seg['start']
    end = seg['end']
    text = seg['text'].strip()
    start_str = format_time(start)
    end_str = format_time(end)
    print(f'[{start_str} - {end_str}] {text}', flush=True)