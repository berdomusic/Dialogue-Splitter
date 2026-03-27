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
        public NumericUpDown NudStartPadding { get; set; }
        public NumericUpDown NudEndPadding { get; set; }
        public Label LblStartPadding { get; set; }
        public Label LblEndPadding { get; set; }
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
        
                var processService = new ProcessService(
                    ExcelService,
                    StatusManager,
                    AudioSelector,
                    ExcelSelector,
                    OutputFolderSelector,
                    Cmb_VO_Text_Column,
                    Cmb_VO_Audio_Column,
                    Tb_SimilarityThreshold,
                    Cmb_Model,
                    Cmb_Language,
                    ChkCreateLogFile,
                    ChkCreateCsvFile,
                    NudStartPadding,
                    NudEndPadding);
        
                await processService.ProcessAsync();
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