using VO_Tool.Status;
using VO_Tool.Selectors;
using VO_Tool.UI;

namespace VO_Tool.Services
{
    public class ProcessService
    {
        private readonly ExcelService _excelService;
        private readonly StatusManager _statusManager;
        private readonly FileSelector _audioSelector;
        private readonly FileSelector _excelSelector;
        private readonly FolderSelector _outputFolderSelector;
        private readonly ComboBox _cmbVoTextColumn;
        private readonly ComboBox _cmbVoAudioColumn;
        private readonly TrackBar _tbSimilarityThreshold;
        private readonly ComboBox _cmbModel;
        private readonly ComboBox _cmbLanguage;
        private readonly CheckBox _chkCreateLogFile;
        private readonly CheckBox _chkCreateCsvFile;
        private readonly NumericUpDown _nudStartPadding;
        private readonly NumericUpDown _nudEndPadding;
        
        public ProcessService(
            ExcelService excelService,
            StatusManager statusManager,
            FileSelector audioSelector,
            FileSelector excelSelector,
            FolderSelector outputFolderSelector,
            ComboBox cmbVoTextColumn,
            ComboBox cmbVoAudioColumn,
            TrackBar tbSimilarityThreshold,
            ComboBox cmbModel,
            ComboBox cmbLanguage,
            CheckBox chkCreateLogFile,
            CheckBox chkCreateCsvFile,
            NumericUpDown nudStartPadding,
            NumericUpDown nudEndPadding)
        {
            _excelService = excelService;
            _statusManager = statusManager;
            _audioSelector = audioSelector;
            _excelSelector = excelSelector;
            _outputFolderSelector = outputFolderSelector;
            _cmbVoTextColumn = cmbVoTextColumn;
            _cmbVoAudioColumn = cmbVoAudioColumn;
            _tbSimilarityThreshold = tbSimilarityThreshold;
            _cmbModel = cmbModel;
            _cmbLanguage = cmbLanguage;
            _chkCreateLogFile = chkCreateLogFile;
            _chkCreateCsvFile = chkCreateCsvFile;
            _nudStartPadding = nudStartPadding;
            _nudEndPadding = nudEndPadding;
        }
        
        public async Task ProcessAsync()
        {
            var startTime = DateTime.Now;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            double similarityThreshold = WhisperService.GetSimilarityThreshold(_tbSimilarityThreshold);
            _statusManager.UpdateStatus($"Similarity threshold: {similarityThreshold:P0}");
            await Task.Delay(500);
            
            int textColumnNum = await _excelService.GetColumnNumberFromLetter(_cmbVoTextColumn.SelectedItem?.ToString() ?? "A");
            int audioColumnNum = await _excelService.GetColumnNumberFromLetter(_cmbVoAudioColumn.SelectedItem?.ToString() ?? "A");
            
            // Read text from Excel
            _statusManager.UpdateStatus($"Reading text from column {_cmbVoTextColumn.SelectedItem}...");
            var texts = await _excelService.ReadColumnByNumberAsync(_excelSelector.FilePath, textColumnNum);
            _statusManager.UpdateStatus($"Found {texts.Count} text entries");

            // Show each text entry with a small delay
            _statusManager.UpdateStatus("=== TEXT ENTRIES ===", false);
            for (int i = 0; i < texts.Count; i++)
            {
                _statusManager.UpdateStatus($"[{i + 1}] {texts[i]}", false);
                await Task.Delay(25);
            }

            await Task.Delay(500);
            
            // Read audio file names from Excel
            _statusManager.UpdateStatus($"Reading file names from column {_cmbVoAudioColumn.SelectedItem}...");
            var audioFileNames = await _excelService.ReadColumnByNumberAsync(_excelSelector.FilePath, audioColumnNum);
            _statusManager.UpdateStatus($"Found {audioFileNames.Count} file names");
            await Task.Delay(500);
            
            // Get selected model
            var selectedModel = _cmbModel.SelectedItem is WhisperModel model ? model : WhisperModel.Base;
            var selectedLanguage = _cmbLanguage.SelectedItem is WhisperLanguage language ? language : WhisperLanguage.English;
            var modelName = selectedModel.ToModelString();
            var languageName = selectedLanguage == WhisperLanguage.Auto ? "Auto" : selectedLanguage.ToString();

            // Transcribe audio
            _statusManager.UpdateStatus("Starting Whisper transcription...");
            var segments = await WhisperService.TranscribeAsync(_audioSelector.FilePath, selectedModel, selectedLanguage, texts, (msg) =>
            {
                _statusManager.UpdateStatus(msg);
            });
            _statusManager.UpdateStatus($"Transcription complete. Found {segments.Count} speech segments");
            await Task.Delay(500);
            
            // Matching text
            _statusManager.UpdateStatus("Matching transcribed segments to Excel texts...");
            var matches = TextMatchingService.MatchSegmentsToTexts(
                segments, texts, audioFileNames, similarityThreshold);

            int matchedCount = matches.Count(m => m.IsMatch);
            int totalTexts = texts.Count;
            int totalSegments = segments.Count;

            _statusManager.UpdateStatus($"Matched {matchedCount} of {totalTexts} texts (Segments: {totalSegments})");
            
            // Get padding values
            double startPadding = (double)_nudStartPadding.Value;
            double endPadding = (double)_nudEndPadding.Value;
            
            // Create CSV data with padding
            string csvData = CsvService.CreateMatchesCsvData(matches, startPadding, endPadding);
            
            // Show match summary
            _statusManager.UpdateStatus("=== MATCH SUMMARY ===");

            try
            {
                foreach (var match in matches)
                {
                    if (match.Segment != null)
                    {
                        string status = match.IsMatch ? "✓" : "✗";
                        string start = UIHelpers.FormatTime(match.Segment.Start);
                        string end = UIHelpers.FormatTime(match.Segment.End);
                        _statusManager.UpdateStatus($"{status} [{start} - {end}] '{match.Segment.Text}' -> '{match.ExpectedText}' ({match.Similarity:P0})");
                    }
                    else
                    {
                        _statusManager.UpdateStatus($"✗ MISSING: Text '{match.ExpectedText}' had no matching segment");
                    }
                }
            }
            catch (Exception ex)
            {
                _statusManager.UpdateStatus($"ERROR in match summary: {ex.Message}");
            }
            await Task.Delay(500);
            
            var endTime = DateTime.Now;
            var totalSeconds = (endTime - startTime).TotalSeconds;
            _statusManager.UpdateStatus($"Total processing time: {totalSeconds:F1} seconds");
            
            _statusManager.UpdateStatus("=== PROCESS COMPLETE ===");
            
            // Create folder if any output is enabled
            bool shouldCreateFolder = _chkCreateLogFile.Checked || _chkCreateCsvFile.Checked;

            if (shouldCreateFolder)
            {
                var folderName = $"log_{timestamp}_{modelName}_{languageName}";
                var outputFolder = Path.Combine(_outputFolderSelector.FolderPath, folderName);
                Directory.CreateDirectory(outputFolder);
                
                // Save log file
                if (_chkCreateLogFile.Checked)
                {
                    var logPath = Path.Combine(outputFolder, $"split_log_{timestamp}_{modelName}_{languageName}.txt");
                    var logMessages = _statusManager.GetLogMessages();
                    
                    using (var writer = new StreamWriter(logPath))
                    {
                        writer.WriteLine("=== VO Audio Splitter Log ===");
                        writer.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        writer.WriteLine($"Audio file: {_audioSelector.FilePath}");
                        writer.WriteLine($"Excel file: {_excelSelector.FilePath}");
                        writer.WriteLine($"Text column: {_cmbVoTextColumn.SelectedItem?.ToString() ?? string.Empty}");
                        writer.WriteLine($"Audio file name column: {_cmbVoAudioColumn.SelectedItem?.ToString() ?? string.Empty}");
                        writer.WriteLine($"Whisper model: {modelName}");
                        writer.WriteLine($"Language: {languageName}");
                        writer.WriteLine();
                        writer.WriteLine("=== All Status Messages ===");
                        
                        foreach (var msg in logMessages)
                        {
                            writer.WriteLine(msg);
                        }
                        
                        writer.WriteLine();
                        writer.WriteLine("=== End of Log ===");
                    }
                    
                    _statusManager.UpdateStatus($"Log file saved to output folder");
                    await Task.Delay(500);
                }
                
                // Save CSV file
                if (_chkCreateCsvFile.Checked && !string.IsNullOrEmpty(csvData))
                {
                    var csvPath = Path.Combine(outputFolder, $"matches_{timestamp}.csv");
                    File.WriteAllText(csvPath, csvData, System.Text.Encoding.UTF8);
                    _statusManager.UpdateStatus($"Match report CSV saved to output folder");
                    await Task.Delay(500);
                }
            }
            
            UIHelpers.ShowSuccess(
                $"Successfully processed:\n" +
                $"- {texts.Count} text entries\n" +
                $"- {segments.Count} speech segments detected\n" +
                $"- {matchedCount} matches above threshold\n" +
                $"- Output folder: {_outputFolderSelector.FolderPath}"
            );
        }
    }
}