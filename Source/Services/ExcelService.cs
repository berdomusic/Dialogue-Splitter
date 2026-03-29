using OfficeOpenXml;
using VO_Tool.Status;
using VO_tool.UI;
using VO_Tool.UI;

namespace VO_Tool.Services
{
    public class ExcelService
    {
        public ExcelService()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Dialogue Splitter");
        }
        
        public Task<List<string>> GetColumnLettersWithDataAsync(string excelPath)
        {
            var columnLetters = new List<string>();
            
            using (var package = new ExcelPackage(new FileInfo(excelPath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var dimension = worksheet.Dimension;
                
                if (dimension == null)
                {
                    return Task.FromResult(columnLetters);
                }
                
                for (int col = dimension.Start.Column; col <= dimension.End.Column; col++)
                {
                    bool hasData = false;
                    
                    for (int row = dimension.Start.Row; row <= dimension.End.Row; row++)
                    {
                        var cell = worksheet.Cells[row, col];
                        if (cell.Value != null && !string.IsNullOrWhiteSpace(cell.Value.ToString()))
                        {
                            hasData = true;
                            break;
                        }
                    }
                    
                    if (hasData)
                    {
                        columnLetters.Add(GetColumnLetter(col));
                    }
                }
            }
            
            return Task.FromResult(columnLetters);
        }
        
        public async Task LoadExcelColumnsAsync(string filePath, ComboBox textColumnCombo, ComboBox audioColumnCombo, StatusManager statusManager, Action<string, string>? onColumnsLoaded = null)
        {
            if (!File.Exists(filePath) || !UiHelpers.IsExcelFile(filePath)) return;
            
            try
            {
                statusManager.UpdateStatus("Loading Excel columns with data...");
                
                var columnLetters = await GetColumnLettersWithDataAsync(filePath);
                
                if (columnLetters.Count == 0)
                {
                    statusManager.UpdateStatus("No data found in Excel file.");
                    return;
                }
                
                UiBuilder.PopulateComboBox(textColumnCombo, columnLetters);
                UiBuilder.PopulateComboBox(audioColumnCombo, columnLetters);
                
                textColumnCombo.Enabled = true;
                audioColumnCombo.Enabled = true;
                
                statusManager.UpdateStatus($"Found columns with data: {string.Join(", ", columnLetters)}");
                
                onColumnsLoaded?.Invoke(textColumnCombo.SelectedItem?.ToString() ?? string.Empty, 
                                       audioColumnCombo.SelectedItem?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                statusManager.UpdateStatus($"Error loading Excel: {ex.Message}");
                UiHelpers.ShowException($"Error loading Excel: {ex.Message}");
            }
        }
        
        private string GetColumnLetter(int columnNumber)
        {
            string result = "";
            while (columnNumber > 0)
            {
                columnNumber--;
                result = (char)('A' + columnNumber % 26) + result;
                columnNumber /= 26;
            }
            return result;
        }
        
        public Task<List<string>> ReadColumnByNumberAsync(string excelPath, int columnNumber)
        {
            var texts = new List<string>();
            
            using (var package = new ExcelPackage(new FileInfo(excelPath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var dimension = worksheet.Dimension;
                
                if (dimension == null)
                {
                    throw new Exception("Excel file is empty.");
                }
                
                for (int row = dimension.Start.Row; row <= dimension.End.Row; row++)
                {
                    var cell = worksheet.Cells[row, columnNumber];
                    if (cell.Value != null && !string.IsNullOrWhiteSpace(cell.Value.ToString()))
                    {
                        texts.Add(cell.Value.ToString()!.Trim());
                    }
                }
            }
            
            if (texts.Count == 0)
            {
                throw new Exception($"No text found in column {GetColumnLetter(columnNumber)}.");
            }
            
            return Task.FromResult(texts);
        }
        
        public Task<int> GetColumnNumberFromLetter(string columnLetter)
        {
            int result = 0;
            foreach (char c in columnLetter.ToUpper())
            {
                result = result * 26 + (c - 'A' + 1);
            }
            return Task.FromResult(result);
        }
    }
}