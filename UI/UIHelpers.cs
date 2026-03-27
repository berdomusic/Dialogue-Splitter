using VO_Tool.Status;

namespace VO_Tool.UI
{
    public static class UIHelpers
    {
        // ============ Validation ============
        public static bool ValidateInputs(UIControls ui, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(ui.ExcelSelector.FilePath))
            {
                errorMessage = "Please select an Excel file.";
                return false;
            }

            if (string.IsNullOrEmpty(ui.AudioSelector.FilePath))
            {
                errorMessage = "Please select an audio file.";
                return false;
            }
            
            if (string.IsNullOrEmpty(ui.OutputFolderSelector.FolderPath))
            {
                errorMessage = "Please select an output folder.";
                return false;
            }

            if (!File.Exists(ui.ExcelSelector.FilePath))
            {
                errorMessage = "Excel file does not exist.";
                return false;
            }

            if (!File.Exists(ui.AudioSelector.FilePath))
            {
                errorMessage = "Audio file does not exist.";
                return false;
            }

            if (ui.Cmb_VO_Text_Column.SelectedItem == null)
            {
                errorMessage = "Please select a VO text column.";
                return false;
            }

            if (ui.Cmb_VO_Audio_Column.SelectedItem == null)
            {
                errorMessage = "Please select a VO audio file name column.";
                return false;
            }

            return true;
        }
        
        // ============ Message Boxes ============
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Validation Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        public static void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        public static void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Success", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        public static void ShowException(string message)
        {
            MessageBox.Show(message, "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        // ============ File Type Detection ============
        public static bool IsExcelFile(string filePath)
        {
            return filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || 
                   filePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase);
        }
        
        public static bool IsAudioFile(string filePath)
        {
            var helper = new FileSelectionHelper();
            return helper.IsAudioFile(filePath);
        }
        
        public static string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }
        
        // ============ Time Formatting ============
        public static string FormatTime(double seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            
            if (ts.TotalHours >= 1)
            {
                return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}";
            }
            else
            {
                return $"{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}";
            }
        }
        
        // ============ Async Operation Helpers ============
        public static async Task<T> ExecuteWithStatusAsync<T>(StatusManager status, Func<Task<T>> action, string loadingMessage, string successMessage)
        {
            status.UpdateStatus(loadingMessage);
            try
            {
                var result = await action();
                status.UpdateStatus(successMessage);
                return result;
            }
            catch
            {
                status.UpdateStatus("Operation failed");
                throw;
            }
        }
        
        public static async Task ExecuteWithStatusAsync(StatusManager status, Func<Task> action, string loadingMessage, string successMessage)
        {
            status.UpdateStatus(loadingMessage);
            try
            {
                await action();
                status.UpdateStatus(successMessage);
            }
            catch
            {
                status.UpdateStatus("Operation failed");
                throw;
            }
        }
    }
}