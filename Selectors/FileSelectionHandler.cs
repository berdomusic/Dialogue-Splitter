using VO_Tool.Services;
using VO_Tool.Status;

namespace VO_Tool.UI
{
    public class FileSelectionHandler
    {
        private readonly StatusManager statusManager;
        private readonly ExcelService excelService;
        private readonly ReaperService reaperService;
        
        public FileSelectionHandler(StatusManager statusManager, ExcelService excelService, ReaperService reaperService)
        {
            this.statusManager = statusManager;
            this.excelService = excelService;
            this.reaperService = reaperService;
        }
        
        public async Task HandleExcelFileAsync(string filePath, ComboBox textColumnCombo, ComboBox audioColumnCombo)
        {
            if (!File.Exists(filePath) || !UIHelpers.IsExcelFile(filePath)) return;
            
            try
            {
                statusManager.UpdateStatus("Loading Excel columns with data...");
                
                // Get only columns that have data
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
        
        public async Task HandleReaperFileAsync(string filePath, ComboBox sourceTrackCombo, ComboBox outputTrackCombo)
        {
            if (!File.Exists(filePath) || !UIHelpers.IsReaperFile(filePath)) return;
    
            try
            {
                statusManager.UpdateStatus("Loading Reaper tracks...");
                
                var tracks = await reaperService.GetTrackNamesAsync(filePath);
                
                if (tracks.Count == 0)
                {
                    statusManager.UpdateStatus("No tracks found in Reaper project.");
                    return;
                }
        
                UIHelpers.PopulateComboBox(sourceTrackCombo, tracks);
                UIHelpers.PopulateComboBox(outputTrackCombo, tracks);
        
                UIHelpers.EnableControls(sourceTrackCombo, outputTrackCombo);
                
                statusManager.UpdateStatus($"Loaded {tracks.Count} tracks");
            }
            catch (Exception ex)
            {
                statusManager.UpdateStatus($"Error loading Reaper: {ex.Message}");
                UIHelpers.ShowException($"Error loading Reaper: {ex.Message}");
            }
        }
    }
}