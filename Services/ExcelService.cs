using OfficeOpenXml;

namespace VO_Tool.Services
{
    public class ExcelService
    {
        public ExcelService()
        {
            // EPPlus 8+ license configuration
            ExcelPackage.License.SetNonCommercialPersonal("VO_Tool - Audio Splitter Tool");
        }
        
        public async Task<List<string>> GetColumnHeadersAsync(string excelPath)
        {
            var headers = new List<string>();
            
            using (var package = new ExcelPackage(new FileInfo(excelPath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int totalColumns = worksheet.Dimension?.Columns ?? 0;
                
                for (int col = 1; col <= totalColumns; col++)
                {
                    var header = worksheet.Cells[1, col].Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(header))
                    {
                        headers.Add(header.Trim());
                    }
                }
            }
            
            return await Task.FromResult(headers);
        }
        
        public async Task<List<string>> ReadTextColumnAsync(string excelPath, string columnName)
        {
            var texts = new List<string>();
            
            using (var package = new ExcelPackage(new FileInfo(excelPath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                
                // Find the column by name
                int columnIndex = FindColumnIndex(worksheet, columnName);
                
                if (columnIndex == -1)
                {
                    string availableColumns = GetAvailableColumns(worksheet);
                    throw new Exception(
                        $"Column '{columnName}' not found. Available columns: {availableColumns}"
                    );
                }
                
                // Read all rows
                int totalRows = worksheet.Dimension?.Rows ?? 0;
                for (int row = 2; row <= totalRows; row++)
                {
                    var cellValue = worksheet.Cells[row, columnIndex].Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        texts.Add(cellValue.Trim());
                    }
                }
            }
            
            if (texts.Count == 0)
            {
                throw new Exception($"No text found in column '{columnName}'.");
            }
            
            return await Task.FromResult(texts);
        }
        
        private int FindColumnIndex(ExcelWorksheet worksheet, string columnName)
        {
            int totalColumns = worksheet.Dimension?.Columns ?? 0;
            
            for (int col = 1; col <= totalColumns; col++)
            {
                var header = worksheet.Cells[1, col].Value?.ToString();
                if (string.Equals(header, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return col;
                }
            }
            
            return -1;
        }
        
        private string GetAvailableColumns(ExcelWorksheet worksheet)
        {
            var columns = new List<string>();
            int totalColumns = worksheet.Dimension?.Columns ?? 0;
            
            for (int col = 1; col <= totalColumns; col++)
            {
                var header = worksheet.Cells[1, col].Value?.ToString();
                if (!string.IsNullOrEmpty(header))
                {
                    columns.Add(header);
                }
            }
            
            return columns.Count > 0 ? string.Join(", ", columns) : "No headers found";
        }
    }
}