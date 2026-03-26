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
                DisableControls(BtnProcess);
                StatusManager.UpdateStatus("=== PROCESS START ===");
                
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

                // Transcribe audio with selected model and prompt from Excel texts
                StatusManager.UpdateStatus("Starting Whisper transcription...");
                var segments = await WhisperService.TranscribeAsync(AudioSelector.FilePath, selectedModel, selectedLanguage, texts, (msg) =>
                {
                    StatusManager.UpdateStatus(msg);
                });
                StatusManager.UpdateStatus($"Transcription complete. Found {segments.Count} speech segments");
                await Task.Delay(500);
                
                if (ChkCreateLogFile.Checked)
                {
                    StatusManager.SaveLogToFile(
                        OutputFolderSelector.FolderPath,
                        AudioSelector.FilePath,
                        ExcelSelector.FilePath,
                        Cmb_VO_Text_Column.SelectedItem?.ToString() ?? string.Empty,
                        Cmb_VO_Audio_Column.SelectedItem?.ToString() ?? string.Empty,
                        selectedModel,
                        selectedLanguage
                    );
                    StatusManager.UpdateStatus($"Log file saved to output folder");
                    await Task.Delay(500);
                }
                
                StatusManager.UpdateStatus("=== PROCESS COMPLETE ===");
                
                UIHelpers.ShowSuccess(
                    $"Successfully processed:\n" +
                    $"- {texts.Count} text entries\n" +
                    $"- {segments.Count} speech segments detected\n" +
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