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
                var columns = await UIHelpers.ExecuteWithStatusAsync(
                    statusManager,
                    () => excelService.GetColumnHeadersAsync(filePath),
                    "Loading Excel columns...",
                    $"Loaded columns from {UIHelpers.GetFileName(filePath)}"
                );
                
                // Populate both combo boxes with the same columns
                UIHelpers.PopulateComboBox(textColumnCombo, columns);
                UIHelpers.PopulateComboBox(audioColumnCombo, columns);
                
                UIHelpers.EnableControls(textColumnCombo, audioColumnCombo);
            }
            catch (Exception ex)
            {
                UIHelpers.ShowException($"Error loading Excel: {ex.Message}");
            }
        }
        
        public async Task HandleReaperFileAsync(string filePath, ComboBox sourceTrackCombo, ComboBox outputTrackCombo)
        {
            if (!File.Exists(filePath) || !UIHelpers.IsReaperFile(filePath)) return;
    
            try
            {
                var tracks = await UIHelpers.ExecuteWithStatusAsync(
                    statusManager,
                    () => reaperService.GetTrackNamesAsync(filePath),
                    "Loading Reaper tracks...",
                    $"Loaded {UIHelpers.GetFileName(filePath)}"
                );
        
                // Populate both combo boxes with the same tracks
                UIHelpers.PopulateComboBox(sourceTrackCombo, tracks);
                UIHelpers.PopulateComboBox(outputTrackCombo, tracks);
        
                UIHelpers.EnableControls(sourceTrackCombo, outputTrackCombo);
            }
            catch (Exception ex)
            {
                UIHelpers.ShowException($"Error loading Reaper: {ex.Message}");
            }
        }
    }
}