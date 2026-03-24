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
        public Label Lbl_ExcelFile { get; set; }
        public Label Lbl_AudioFile { get; set; }
        public Label Lbl_VO_Text_Column { get; set; }
        public ComboBox Cmb_VO_Text_Column { get; set; }
        public Label Lbl_VO_Audio_Column { get; set; }
        public ComboBox Cmb_VO_Audio_Column { get; set; }
        public TrackBar Tb_SimilarityThreshold { get; set; }
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
                
                double similarityThreshold = WhisperService.GetSimilarityThreshold(Tb_SimilarityThreshold);
                StatusManager.UpdateStatus($"Similarity threshold: {similarityThreshold:F2}");
                
                int textColumnNum = await ExcelService.GetColumnNumberFromLetter(Cmb_VO_Text_Column.SelectedItem?.ToString() ?? "A");
                int audioColumnNum = await ExcelService.GetColumnNumberFromLetter(Cmb_VO_Audio_Column.SelectedItem?.ToString() ?? "A");
                
                var texts = await UIHelpers.ExecuteWithStatusAsync(
                    StatusManager,
                    () => ExcelService.ReadColumnByNumberAsync(ExcelSelector.FilePath, textColumnNum),
                    "Reading text from Excel...",
                    $"Found {Cmb_VO_Text_Column.SelectedItem} column entries"
                );
                
                var audioFileNames = await UIHelpers.ExecuteWithStatusAsync(
                    StatusManager,
                    () => ExcelService.ReadColumnByNumberAsync(ExcelSelector.FilePath, audioColumnNum),
                    "Reading audio file names...",
                    $"Found {Cmb_VO_Audio_Column.SelectedItem} column entries"
                );
                
                StatusManager.UpdateStatus($"Loaded {texts.Count} text entries and {audioFileNames.Count} file names");
                
                UIHelpers.ShowSuccess(
                    $"Successfully loaded:\n" +
                    $"- {texts.Count} text entries from column {Cmb_VO_Text_Column.SelectedItem}\n" +
                    $"- {audioFileNames.Count} audio file names from column {Cmb_VO_Audio_Column.SelectedItem}\n" +
                    $"- Audio file: {UIHelpers.GetFileName(AudioSelector.FilePath)}\n" +
                    $"- Similarity threshold: {similarityThreshold:F2}"
                );
            }
            catch (Exception ex)
            {
                StatusManager.UpdateStatus($"Error: {ex.Message}");
                UIHelpers.ShowException($"An error occurred: {ex.Message}");
            }
            finally
            {
                EnableControls(BtnProcess);
            }
        }
    }
}