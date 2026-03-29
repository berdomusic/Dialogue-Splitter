using VO_Tool.Selectors;
using VO_Tool.Status;
using VO_Tool.Services;

namespace VO_Tool.UI
{
    public class UiControls
    {
        public Form MainForm { get; set; } = null!;
        public FileSelector ExcelSelector { get; set; } = null!;
        public FileSelector AudioSelector { get; set; } = null!;
        public FolderSelector OutputFolderSelector { get; set; } = null!;
        public Label LblExcelFile { get; set; } = null!;
        public Label LblAudioFile { get; set; } = null!;
        public Label LblOutputFolder { get; set; } = null!;
        public Label LblVoTextColumn { get; set; } = null!;
        public ComboBox CmbVoTextColumn { get; set; } = null!;
        public Label LblVoAudioColumn { get; set; } = null!;
        public ComboBox CmbVoAudioColumn { get; set; } = null!;
        public TrackBar TbSimilarityThreshold { get; set; } = null!;
        public ComboBox CmbModel { get; set; } = null!;
        public Label LblModel { get; set; } = null!;
        public ComboBox CmbLanguage { get; set; } = null!;
        public Label LblLanguage { get; set; } = null!;
        public CheckBox ChkCreateLogFile { get; set; } = null!;
        public CheckBox ChkCreateCsvFile { get; set; } = null!;
        public CheckBox ChkSplitAudio { get; set; } = null!;
        public NumericUpDown NudStartPadding { get; set; } = null!;
        public NumericUpDown NudEndPadding { get; set; } = null!;
        public Label LblStartPadding { get; set; } = null!;
        public Label LblEndPadding { get; set; } = null!;
        public Button BtnProcess { get; set; } = null!;
        public StatusManager StatusManager { get; set; } = null!;
        public ExcelService ExcelService { get; set; } = null!;
        
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
                if (UiHelpers.IsExcelFile(file))
                {
                    onExcelFile(file);
                }
                else if (UiHelpers.IsAudioFile(file))
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
            if (!UiHelpers.ValidateInputs(this, out string errorMessage))
            {
                UiHelpers.ShowError(errorMessage);
                return;
            }

            try
            {
                StatusManager.ClearLog();
                DisableControls(BtnProcess);
                StatusManager.UpdateStatus("=== PROCESS START ===");

                var processService = new ProcessService(
                    ExcelService,
                    StatusManager,
                    AudioSelector,
                    ExcelSelector,
                    OutputFolderSelector,
                    CmbVoTextColumn,
                    CmbVoAudioColumn,
                    TbSimilarityThreshold,
                    CmbModel,
                    CmbLanguage,
                    ChkCreateLogFile,
                    ChkCreateCsvFile,
                    ChkSplitAudio,  // Add this line
                    NudStartPadding,
                    NudEndPadding);

                await processService.ProcessAsync();
            }
            catch (Exception ex)
            {
                StatusManager.UpdateStatus($"ERROR: {ex.Message}");
                UiHelpers.ShowException($"An error occurred: {ex.Message}");
            }
            finally
            {
                EnableControls(BtnProcess);
            }
        }
    }
}