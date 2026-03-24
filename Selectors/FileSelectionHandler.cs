using VO_Tool.Services;
using VO_Tool.Status;

namespace VO_Tool.UI
{
    public class FileSelectionHandler
    {
        private readonly StatusManager statusManager;
        private readonly ExcelService excelService;
        private readonly FileSelectionHelper fileSelectionHelper;
        
        public FileSelectionHandler(StatusManager statusManager, ExcelService excelService)
        {
            this.statusManager = statusManager;
            this.excelService = excelService;
            this.fileSelectionHelper = new FileSelectionHelper();
        }
        
        public async Task HandleExcelFileAsync(string filePath, ComboBox textColumnCombo, ComboBox audioColumnCombo)
        {
            if (!File.Exists(filePath) || !UIHelpers.IsExcelFile(filePath)) return;
            
            try
            {
                statusManager.UpdateStatus("Loading Excel columns with data...");
                
                var columnLetters = await excelService.GetColumnLettersWithDataAsync(filePath);
                
                if (columnLetters.Count == 0)
                {
                    statusManager.UpdateStatus("No data found in Excel file.");
                    return;
                }
                
                UIHelpers.PopulateComboBox(textColumnCombo, columnLetters);
                UIHelpers.PopulateComboBox(audioColumnCombo, columnLetters);
                
                UIHelpers.EnableControls(textColumnCombo, audioColumnCombo);
                
                statusManager.UpdateStatus($"Found columns with data: {string.Join(", ", columnLetters)}");
            }
            catch (Exception ex)
            {
                statusManager.UpdateStatus($"Error loading Excel: {ex.Message}");
                UIHelpers.ShowException($"Error loading Excel: {ex.Message}");
            }
        }
        
        public void HandleAudioFileSelected(string filePath)
        {
            if (!File.Exists(filePath)) return;
            
            var validation = fileSelectionHelper.ValidateAudioFile(filePath);
            
            if (validation.IsValid)
            {
                statusManager.UpdateStatus($"Audio file: {UIHelpers.GetFileName(filePath)} - {validation.GetStatusMessage()}");
            }
            else
            {
                statusManager.UpdateStatus($"Error: {validation.ErrorMessage}");
                UIHelpers.ShowError(validation.ErrorMessage);
            }
        }
    }
}