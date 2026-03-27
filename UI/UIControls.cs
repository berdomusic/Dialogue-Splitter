using VO_Tool.Selectors;
using VO_Tool.Status;
using VO_Tool.Services;

namespace VO_Tool.UI
{
    public class UIControls
    {
        public Form MainForm { get; set; }
        public FileSelector ExcelSelector { get; set; }
        public FileSelector AudioSelector { get; set; }
        public FolderSelector OutputFolderSelector { get; set; }
        public Label Lbl_ExcelFile { get; set; }
        public Label Lbl_AudioFile { get; set; }
        public Label Lbl_OutputFolder { get; set; }
        public Label Lbl_VO_Text_Column { get; set; }
        public ComboBox Cmb_VO_Text_Column { get; set; }
        public Label Lbl_VO_Audio_Column { get; set; }
        public ComboBox Cmb_VO_Audio_Column { get; set; }
        public TrackBar Tb_SimilarityThreshold { get; set; }
        public ComboBox Cmb_Model { get; set; }
        public Label Lbl_Model { get; set; }
        public ComboBox Cmb_Language { get; set; }
        public Label Lbl_Language { get; set; }
        public CheckBox ChkCreateLogFile { get; set; }
        public CheckBox ChkCreateCsvFile { get; set; }
        public Button BtnProcess { get; set; }
        public StatusManager StatusManager { get; set; }
        
        public ExcelService ExcelService { get; set; }
        
        public static void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        
        public static void OnFileDrop(object? sender, DragEventArgs e, 
            Action<string> onExcelFile, 
            Action<string> onAudioFile,
            Action<string>? onOtherFile)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is not string[] files) return;
            
            foreach (string file in files)
            {
                if (UIHelpers.IsExcelFile(file))
                {
                    onExcelFile(file);
                }
                else if (UIHelpers.IsAudioFile(file))
                {
                    onAudioFile(file);
                }
                else
                {
                    onOtherFile?.Invoke(file);
                }
            }
        }
        
        public static void EnableControls(params Control[] controls)
        {
            foreach (var control in controls)
            {
                control.Enabled = true;
            }
        }
        
        public static void DisableControls(params Control[] controls)
        {
            foreach (var control in controls)
            {
                control.Enabled = false;
            }
        }
        
        public async Task OnProcessClick(object? sender, EventArgs e)
        {
            if (!UIHelpers.ValidateInputs(this, out string errorMessage))
            {
                UIHelpers.ShowError(errorMessage);
                return;
            }
            
            try
            {
                StatusManager.ClearLog();
                
                DisableControls(BtnProcess);
                StatusManager.UpdateStatus("=== PROCESS START ===");
                
                var startTime = DateTime.Now;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                
                double similarityThreshold = WhisperService.GetSimilarityThreshold(Tb_SimilarityThreshold);
                StatusManager.UpdateStatus($"Similarity threshold: {similarityThreshold:P0}");
                await Task.Delay(500);
                
                int textColumnNum = await ExcelService.GetColumnNumberFromLetter(Cmb_VO_Text_Column.SelectedItem?.ToString() ?? "A");
                int audioColumnNum = await ExcelService.GetColumnNumberFromLetter(Cmb_VO_Audio_Column.SelectedItem?.ToString() ?? "A");
                
                // Read text from Excel
                StatusManager.UpdateStatus($"Reading text from column {Cmb_VO_Text_Column.SelectedItem}...");
                var texts = await ExcelService.ReadColumnByNumberAsync(ExcelSelector.FilePath, textColumnNum);
                StatusManager.UpdateStatus($"Found {texts.Count} text entries");

                // Show each text entry with a small delay
                StatusManager.UpdateStatus("=== TEXT ENTRIES ===", false);
                for (int i = 0; i < texts.Count; i++)
                {
                    StatusManager.UpdateStatus($"[{i + 1}] {texts[i]}", false);
                    await Task.Delay(25);
                }

                await Task.Delay(500);
                
                // Read audio file names from Excel
                StatusManager.UpdateStatus($"Reading file names from column {Cmb_VO_Audio_Column.SelectedItem}...");
                var audioFileNames = await ExcelService.ReadColumnByNumberAsync(ExcelSelector.FilePath, audioColumnNum);
                StatusManager.UpdateStatus($"Found {audioFileNames.Count} file names");
                await Task.Delay(500);
                
                // Get selected model
                var selectedModel = Cmb_Model.SelectedItem is WhisperModel model ? model : WhisperModel.Base;
                var selectedLanguage = Cmb_Language.SelectedItem is WhisperLanguage language ? language : WhisperLanguage.English;
                var modelName = selectedModel.ToModelString();
                var languageName = selectedLanguage == WhisperLanguage.Auto ? "Auto" : selectedLanguage.ToString();

                // Transcribe audio with selected model and prompt from Excel texts
                StatusManager.UpdateStatus("Starting Whisper transcription...");
                var segments = await WhisperService.TranscribeAsync(AudioSelector.FilePath, selectedModel, selectedLanguage, texts, (msg) =>
                {
                    StatusManager.UpdateStatus(msg);
                });
                StatusManager.UpdateStatus($"Transcription complete. Found {segments.Count} speech segments");
                await Task.Delay(500);
                
                // Matching text
                StatusManager.UpdateStatus("Matching transcribed segments to Excel texts...");
                var matches = TextMatchingService.MatchSegmentsToTexts(
                    segments, texts, audioFileNames, similarityThreshold);

                int matchedCount = matches.Count(m => m.IsMatch);
                int totalTexts = texts.Count;
                int totalSegments = segments.Count;

                StatusManager.UpdateStatus($"Matched {matchedCount} of {totalTexts} texts (Segments: {totalSegments})");
                
                // Create CSV data (always in memory)
                string csvData = CsvService.CreateMatchesCsvData(matches);
                
                // Show match summary
                StatusManager.UpdateStatus("=== MATCH SUMMARY ===");

                try
                {
                    foreach (var match in matches)
                    {
                        if (match.Segment != null)
                        {
                            string status = match.IsMatch ? "✓" : "✗";
                            StatusManager.UpdateStatus($"{status} [{match.Segment.Start:F1}s - {match.Segment.End:F1}s] '{match.Segment.Text}' -> '{match.ExpectedText}' ({match.Similarity:P0})");
                        }
                        else
                        {
                            StatusManager.UpdateStatus($"✗ MISSING: Text '{match.ExpectedText}' had no matching segment");
                        }
                    }
                }
                catch (Exception ex)
                {
                    StatusManager.UpdateStatus($"ERROR in match summary: {ex.Message}");
                }
                await Task.Delay(500);
                
                var endTime = DateTime.Now;
                var totalSeconds = (endTime - startTime).TotalSeconds;
                StatusManager.UpdateStatus($"Total processing time: {totalSeconds:F1} seconds");
                
                StatusManager.UpdateStatus("=== PROCESS COMPLETE ===");
                
               // Create folder if any output is enabled
                bool shouldCreateFolder = ChkCreateLogFile.Checked || ChkCreateCsvFile.Checked;

                if (shouldCreateFolder)
                {
                    var folderName = $"log_{timestamp}_{modelName}_{languageName}";
                    var outputFolder = Path.Combine(OutputFolderSelector.FolderPath, folderName);
                    Directory.CreateDirectory(outputFolder);
                    
                    // Save log file directly in the folder (not creating another subfolder)
                    if (ChkCreateLogFile.Checked)
                    {
                        var logPath = Path.Combine(outputFolder, $"split_log_{timestamp}_{modelName}_{languageName}.txt");
                        
                        // Get all log messages from StatusManager
                        var logMessages = StatusManager.GetLogMessages(); // You'll need to expose this from StatusManager
                        
                        using (var writer = new StreamWriter(logPath))
                        {
                            writer.WriteLine("=== VO Audio Splitter Log ===");
                            writer.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                            writer.WriteLine($"Audio file: {AudioSelector.FilePath}");
                            writer.WriteLine($"Excel file: {ExcelSelector.FilePath}");
                            writer.WriteLine($"Text column: {Cmb_VO_Text_Column.SelectedItem?.ToString() ?? string.Empty}");
                            writer.WriteLine($"Audio file name column: {Cmb_VO_Audio_Column.SelectedItem?.ToString() ?? string.Empty}");
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
                        
                        StatusManager.UpdateStatus($"Log file saved to output folder");
                        await Task.Delay(500);
                    }
                    
                    // Save CSV file in the same folder
                    if (ChkCreateCsvFile.Checked && !string.IsNullOrEmpty(csvData))
                    {
                        var csvPath = Path.Combine(outputFolder, $"matches_{timestamp}.csv");
                        File.WriteAllText(csvPath, csvData, System.Text.Encoding.UTF8);
                        StatusManager.UpdateStatus($"Match report CSV saved to output folder");
                        await Task.Delay(500);
                    }
                }
                
                UIHelpers.ShowSuccess(
                    $"Successfully processed:\n" +
                    $"- {texts.Count} text entries\n" +
                    $"- {segments.Count} speech segments detected\n" +
                    $"- {matchedCount} matches above threshold\n" +
                    $"- Output folder: {OutputFolderSelector.FolderPath}"
                );
            }
            catch (Exception ex)
            {
                StatusManager.UpdateStatus($"ERROR: {ex.Message}");
                UIHelpers.ShowException($"An error occurred: {ex.Message}");
            }
            finally
            {
                EnableControls(BtnProcess);
            }
        }
    }
}