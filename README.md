# VO Tool - Audio Splitter & Speech-to-Text

A Windows application that uses OpenAI's Whisper to transcribe audio and match segments against expected text from Excel files. Perfect for voice-over work, audio quality assurance, and batch audio processing.

## Table of Contents
- [Installation](#installation)
- [Quick Start](#quick-start)
- [User Interface](#user-interface)
- [Processing Workflow](#processing-workflow)
- [Output Files](#output-files)
- [Settings & Persistence](#settings--persistence)
- [Whisper Models](#whisper-models)
- [Performance Benchmarks](#performance-benchmarks)
- [Troubleshooting](#troubleshooting)

---

## Installation

### Required Dependencies

1. **Install Python 3.12 or later (later versions don't fully support Whisper / Cuda)**
   - Download from: https://www.python.org/downloads/
   - IMPORTANT: Check "Add Python to PATH" during installation

2. **Install Whisper**
   py -m pip install openai-whisper

3. **PyTorch (CPU version - included with Whisper)**
   - Whisper installs CPU version automatically
   - See optional GPU installation below for better performance

### Optional Dependencies

4. **Install Additional Whisper Models**
   
   Whisper comes with 'base' model by default. To download additional models:
   ```cmd
   py -c "import whisper; whisper.load_model('model_name')"
   ```
   
   Replace model_name with:


| Model | Parameters | Disk Size | Speed | Best For |
|-------|------------|-----------|-------|----------|
| tiny | 39M | ~75 MB | Fastest | Quick tests, low-resource devices |
| base | 74M | ~142 MB | Very Fast | General use on CPU |
| small | 244M | ~466 MB | Fast | Good balance of speed/accuracy |
| medium | 769M | ~1.5 GB | Moderate | High accuracy with reasonable resources |
| large-v3 | 1.55B | ~2.9 GB | Slow | Maximum accuracy, GPU recommended |

**Check Installed Models**

```cmd
dir %USERPROFILE%\.cache\whisper
```


5. **Install PyTorch with GPU Support**
   
   First check which CUDA version works with your GPU:
   nvidia-smi
   
   Example for CUDA 12.1:

   ```cmd
   py -m pip install torch torchaudio torchvision --index-url https://download.pytorch.org/whl/cu121
   ```

6. **Install FFmpeg (Required for non-WAV formats)**
   
   The tool supports MP3, FLAC, M4A, OGG, AAC, OPUS, and WEBM formats, but requires FFmpeg:
   
   - Windows (using Chocolatey): choco install ffmpeg
   - Windows (using Scoop): scoop install ffmpeg
   - Mac: brew install ffmpeg
   - Ubuntu/Debian: sudo apt install ffmpeg

---

## Quick Start

1. Launch VO_Tool.exe

2. Select your files:
   - Excel File - Contains expected text and output audio file names
   - Audio File - The source audio to transcribe (WAV, MP3, FLAC, M4A, etc.)
   - Output Folder - Where results will be saved

3. Configure columns:
   - VO Text Column - Column containing the expected spoken text
   - VO Audio File Name Column - Column containing desired output file names

4. Adjust settings (optional):
   - Similarity threshold (default: 75%)
   - Whisper model (auto-detects installed models)
   - Language (59 languages + Auto-detect)
   - Start/End padding for CSV export

5. Click "Process" and wait for completion

---

## User Interface

### File Selectors
- Excel File - Drag & drop or browse for Excel files (.xlsx, .xls)
- Audio File - Drag & drop or browse for audio files
- Output Folder - Choose where to save results

### Column Selection
- VO Text Column (A=1) - Column letters (A, B, C, etc.) containing expected text
- VO Audio File Name Column (A=1) - Column containing output file names

### Processing Settings
- Similarity Threshold (0-100%) - Minimum similarity score required to consider a transcribed segment a match to the expected text.

Lower = more matches (may include incorrect matches)
Higher = fewer matches (only exact or very close matches)

- 100% - Exact match
- 95% - Contains the text
- 70-90% - Levenshtein ratio + word overlap

Recommended: 70-85%

- Whisper Model - Select from installed models

- Language - Select audio language
  - 59 languages supported including English, Polish, Chinese, Japanese, etc.

- Start Padding (s) - Adjust segment start time in CSV export
  - Positive = add time before segment
  - Negative = cut time from beginning
  - Range: -2.00 to +2.00 seconds
  - Default: 0.00 seconds

- End Padding (s) - Adjust segment end time in CSV export
  - Positive = add time after segment
  - Negative = cut time from end
  - Range: -2.00 to +2.00 seconds
  - Default: 0.20 seconds

### Output Options
- Create log file - Saves detailed log with all status messages
- Create CSV file - Exports match report with timestamps and similarity scores
- Split audio files - Splits original audio into individual WAV files using CSV timestamps

---

## Processing Workflow

1. Load Excel Data
   - Reads text from selected column
   - Reads audio file names from selected column
   - Displays all text entries in status window

2. Transcribe Audio
   - Calls Whisper with selected model and language
   - Shows real-time transcription progress
   - Detects speech segments with timestamps (MM:SS.mmm format)

3. Match Segments
   - Compares each transcribed segment against all expected texts
   - Calculates similarity using Levenshtein distance + word overlap
   - Marks matches above threshold

4. Generate Output
   - Creates log folder: log_YYYYMMDD_HHMMSS_model_language/
   - Saves log file: split_log_YYYYMMDD_HHMMSS_model_language.txt
   - Saves CSV file (if enabled): matches_YYYYMMDD_HHMMSS.csv
   - Splits audio (if enabled): Media/ folder with individual WAV files

5. Show Results
   - Match summary with ✓/✗ indicators
   - Similarity percentages
   - Total processing time

---

## Output Files

### Log File (split_log_*.txt)
Contains:
- Processing date and time
- Input file paths
- Selected columns
- Whisper model and language
- All status messages with timestamps
- Transcription output with formatted timestamps (MM:SS.mmm)
- Match summary with similarity scores
- Total processing time

### CSV File (matches_*.csv)
Columns:
- Source Audio File - Original audio file name
- Start - Segment start time (MM:SS.mmm)
- End - Segment end time (MM:SS.mmm)
- Audio File Name - Output filename from Excel
- Expected Text - Expected text from Excel
- Transcribed Text - What Whisper recognized
- Similarity - Match score (0.0000 - 1.0000)

### Split Audio Files (Media/ folder)
- Individual WAV files for each matched segment
- Filenames from Excel's audio file name column
- Duplicate filenames get suffixes: _1, _2, etc.
- Example: voice_line.wav, voice_line_1.wav, voice_line_2.wav

---

## Settings & Persistence

All settings are automatically saved to:
%LocalAppData%\VO_Tool\settings.json

Saved settings include:
- Last used Excel file, audio file, output folder
- Selected text and audio file name columns
- Similarity threshold
- Whisper model and language
- Log, CSV, and split audio preferences
- Start and end padding values

Settings persist between application restarts.

---


## Performance Benchmarks

### English Language
Test environment: NVIDIA GeForce RTX 3050 (4GB VRAM)
Audio duration: ~2 minutes 20 seconds
All models using FP16 with GPU acceleration

| Metric | Tiny | Base | Small | Medium | Large |
|--------|------|------|-------|--------|-------|
| Transcription Time | ~16.5 sec | ~27.5 sec | ~37.5 sec | ~83.8 sec | ~671.3 sec |
| Total Processing Time | 20.6 sec | 34.6 sec | 41.1 sec | 91.9 sec | 686.3 sec |
| Real-time Factor | 8.1x | 4.8x | 4.1x | 1.6x | 0.2x |
| Relative Speed | 40% faster | Baseline | 16% slower | 2.7x slower | 20x slower |
| Segments Found | 61 | 59 | 54 | 58 | 76 |
| Matched Segments | 58/20 | 58/20 | 51/20 | 58/20 | 61/20 |
| Accuracy | ~95% | ~98% | ~94% | ~98% | ~98% |

### Accuracy Observations

| Model | Strengths | Weaknesses |
|-------|-----------|------------|
| Tiny | Fastest (20.6s total), 61 segments detected | Splits combined phrases (e.g., "Good morning" + "How did you sleep?"), lower accuracy on some phrases |
| Base | Good balance (34.6s), all 20 texts matched, clean punctuation | Slight formatting variations (e.g., "3pm" vs "3 PM") |
| Small | Decent speed (41.1s) | More unmatched segments (51/20), some phrase splitting |
| Medium | High accuracy, proper punctuation, handles contractions well | 3x slower than base (91.9s) |
| Large | Maximum accuracy, best punctuation | Extremely slow (686s / 11.4 minutes), detects extra segments (hallucinations), not practical for real-time use |

---

## Troubleshooting

### "Python is not installed or not in PATH"
- Reinstall Python and ensure "Add Python to PATH" is checked
- Or manually add Python to system PATH

### "Whisper is not installed"
- Run: py -m pip install openai-whisper
- Verify installation: py -c "import whisper; print('OK')"

### "FP16 is not supported on CPU"
- Normal warning when running on CPU
- Ignore or install PyTorch with CUDA for GPU acceleration

### No audio segments detected
- Check audio file format (WAV recommended for best results)
- Ensure audio contains clear speech
- Try lowering similarity threshold

### Out of memory error with large models
- Use smaller model (tiny or base)
- Close other applications
- Process shorter audio segments

### Slow transcription on GPU
- Verify CUDA is working: py -c "import torch; print(torch.cuda.is_available())"
- Should return True if GPU is properly configured

---

## Support

For issues or feature requests, please check the application log file for detailed error information.