using VO_Tool.Status;
using VO_Tool.Selectors;
using VO_Tool.UI;

namespace VO_Tool.Services
{
    public class ProcessService
    {
        private readonly ExcelService excelService;
        private readonly StatusManager statusManager;
        private readonly FileSelector audioSelector;
        private readonly FileSelector excelSelector;
        private readonly FolderSelector outputFolderSelector;
        private readonly ComboBox cmbVoTextColumn;
        private readonly ComboBox cmbVoAudioColumn;
        private readonly TrackBar tbSimilarityThreshold;
        private readonly ComboBox cmbModel;
        private readonly ComboBox cmbLanguage;
        private readonly CheckBox chkCreateLogFile;
        private readonly CheckBox chkCreateCsvFile;
        private readonly CheckBox chkSplitAudio;
        private readonly NumericUpDown nudStartPadding;
        private readonly NumericUpDown nudEndPadding;
        
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
            CheckBox chkSplitAudio,
            NumericUpDown nudStartPadding,
            NumericUpDown nudEndPadding)
        {
            this.excelService = excelService;
            this.statusManager = statusManager;
            this.audioSelector = audioSelector;
            this.excelSelector = excelSelector;
            this.outputFolderSelector = outputFolderSelector;
            this.cmbVoTextColumn = cmbVoTextColumn;
            this.cmbVoAudioColumn = cmbVoAudioColumn;
            this.tbSimilarityThreshold = tbSimilarityThreshold;
            this.cmbModel = cmbModel;
            this.cmbLanguage = cmbLanguage;
            this.chkCreateLogFile = chkCreateLogFile;
            this.chkCreateCsvFile = chkCreateCsvFile;
            this.chkSplitAudio = chkSplitAudio;
            this.nudStartPadding = nudStartPadding;
            this.nudEndPadding = nudEndPadding;
        }
        
        public async Task ProcessAsync()
        {
            var startTime = DateTime.Now;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            double similarityThreshold = WhisperService.GetSimilarityThreshold(tbSimilarityThreshold);
            statusManager.UpdateStatus($"Similarity threshold: {similarityThreshold:P0}");
            await Task.Delay(500);
            
            int textColumnNum = await excelService.GetColumnNumberFromLetter(cmbVoTextColumn.SelectedItem?.ToString() ?? "A");
            int audioColumnNum = await excelService.GetColumnNumberFromLetter(cmbVoAudioColumn.SelectedItem?.ToString() ?? "A");
            
            // Read text from Excel
            statusManager.UpdateStatus($"Reading text from column {cmbVoTextColumn.SelectedItem}...");
            var texts = await excelService.ReadColumnByNumberAsync(excelSelector.FilePath, textColumnNum);
            statusManager.UpdateStatus($"Found {texts.Count} text entries");

            // Show each text entry with a small delay
            statusManager.UpdateStatus("=== TEXT ENTRIES ===", false);
            for (int i = 0; i < texts.Count; i++)
            {
                statusManager.UpdateStatus($"[{i + 1}] {texts[i]}", false);
                await Task.Delay(25);
            }

            await Task.Delay(500);
            
            // Read audio file names from Excel
            statusManager.UpdateStatus($"Reading file names from column {cmbVoAudioColumn.SelectedItem}...");
            var audioFileNames = await excelService.ReadColumnByNumberAsync(excelSelector.FilePath, audioColumnNum);
            statusManager.UpdateStatus($"Found {audioFileNames.Count} file names");
            await Task.Delay(500);
            
            // Get selected model
            var selectedModel = cmbModel.SelectedItem is WhisperModel model ? model : WhisperModel.Base;
            var selectedLanguage = cmbLanguage.SelectedItem is WhisperLanguage language ? language : WhisperLanguage.English;
            var modelName = selectedModel.ToModelString();
            var languageName = selectedLanguage == WhisperLanguage.Auto ? "Auto" : selectedLanguage.ToString();

            // Transcribe audio
            statusManager.UpdateStatus("Starting Whisper transcription...");
            var segments = await WhisperService.TranscribeAsync(audioSelector.FilePath, selectedModel, selectedLanguage, texts, (msg) =>
            {
                statusManager.UpdateStatus(msg);
            });
            statusManager.UpdateStatus($"Transcription complete. Found {segments.Count} speech segments");
            await Task.Delay(500);
            
            // Matching text
            statusManager.UpdateStatus("Matching transcribed segments to Excel texts...");
            var matches = TextMatchingService.MatchSegmentsToTexts(
                segments, texts, audioFileNames, similarityThreshold);

            int matchedCount = matches.Count(m => m.IsMatch);
            int totalTexts = texts.Count;
            int totalSegments = segments.Count;

            statusManager.UpdateStatus($"Matched {matchedCount} of {totalTexts} texts (Segments: {totalSegments})");
            
            // Get padding values
            double startPadding = (double)nudStartPadding.Value;
            double endPadding = (double)nudEndPadding.Value;
            
            // Create CSV data with padding
            string csvData = CsvService.CreateMatchesCsvData(matches, startPadding, endPadding);
            
            // Show match summary
            statusManager.UpdateStatus("=== MATCH SUMMARY ===");

            try
            {
                foreach (var match in matches)
                {
                    if (match.Segment != null)
                    {
                        string status = match.IsMatch ? "✓" : "✗";
                        string start = UiHelpers.FormatTime(match.Segment.Start);
                        string end = UiHelpers.FormatTime(match.Segment.End);
                        statusManager.UpdateStatus($"{status} [{start} - {end}] '{match.Segment.Text}' -> '{match.ExpectedText}' ({match.Similarity:P0})");
                    }
                    else
                    {
                        statusManager.UpdateStatus($"✗ MISSING: Text '{match.ExpectedText}' had no matching segment");
                    }
                }
            }
            catch (Exception ex)
            {
                statusManager.UpdateStatus($"ERROR in match summary: {ex.Message}");
            }
            await Task.Delay(500);
            
            var endTime = DateTime.Now;
            var totalSeconds = (endTime - startTime).TotalSeconds;
            statusManager.UpdateStatus($"Total processing time: {totalSeconds:F1} seconds");
            
            statusManager.UpdateStatus("=== PROCESS COMPLETE ===");
            
            // Create folder if any output is enabled
            bool shouldCreateFolder = chkCreateLogFile.Checked || chkCreateCsvFile.Checked || chkSplitAudio.Checked;

            if (shouldCreateFolder)
            {
                var folderName = $"log_{timestamp}_{modelName}_{languageName}";
                var outputFolder = Path.Combine(outputFolderSelector.FolderPath, folderName);
                Directory.CreateDirectory(outputFolder);
                
                string csvPath = null;
                
                // Save CSV file temporarily
                if (!string.IsNullOrEmpty(csvData))
                {
                    csvPath = Path.Combine(outputFolder, $"matches_{timestamp}.csv");
                    File.WriteAllText(csvPath, csvData, System.Text.Encoding.UTF8);
                    
                    if (chkCreateCsvFile.Checked)
                    {
                        statusManager.UpdateStatus($"Match report CSV saved to output folder");
                    }
                    else
                    {
                        statusManager.UpdateStatus($"Temporary CSV created", false);
                    }
                    await Task.Delay(500);
                }
                
                // Split audio if checkbox is checked
                if (chkSplitAudio.Checked && !string.IsNullOrEmpty(csvData) && csvPath != null && File.Exists(csvPath))
                {
                    var splitter = new AudioSplitterService();
                    await splitter.SplitAudioFromCsv(csvPath, audioSelector.FilePath, outputFolder, statusManager);
                    statusManager.UpdateStatus($"Audio split completed. Files saved to Media folder");
                    await Task.Delay(500);
                }
                
                // Delete temporary CSV if user didn't want it
                if (!chkCreateCsvFile.Checked && csvPath != null && File.Exists(csvPath))
                {
                    File.Delete(csvPath);
                    statusManager.UpdateStatus($"Temporary CSV deleted", false);
                    await Task.Delay(500);
                }
                
                // Save log file
                if (chkCreateLogFile.Checked)
                {
                    var logPath = Path.Combine(outputFolder, $"split_log_{timestamp}_{modelName}_{languageName}.txt");
                    var logMessages = statusManager.GetLogMessages();
                    
                    using (var writer = new StreamWriter(logPath))
                    {
                        writer.WriteLine("=== Dialogue Splitter Log ===");
                        writer.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        writer.WriteLine($"Audio file: {audioSelector.FilePath}");
                        writer.WriteLine($"Excel file: {excelSelector.FilePath}");
                        writer.WriteLine($"Text column: {cmbVoTextColumn.SelectedItem?.ToString() ?? string.Empty}");
                        writer.WriteLine($"Audio file name column: {cmbVoAudioColumn.SelectedItem?.ToString() ?? string.Empty}");
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
                    
                    statusManager.UpdateStatus($"Log file saved to output folder");
                    await Task.Delay(500);
                }
            }
            
            UiHelpers.ShowSuccess(
                $"Successfully processed:\n" +
                $"- {texts.Count} text entries\n" +
                $"- {segments.Count} speech segments detected\n" +
                $"- {matchedCount} matches above threshold\n" +
                $"- Output folder: {outputFolderSelector.FolderPath}"
            );
        }
    }
}