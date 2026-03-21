using OfficeOpenXml;

namespace VO_Tool.Services
{
    public class ExcelService
    {
        public ExcelService()
        {
            ExcelPackage.License.SetNonCommercialPersonal("VO_Tool - Audio Splitter Tool");
        }
        
        public Task<List<string>> GetColumnLettersWithDataAsync(string excelPath)
        {
            var columnLetters = new List<string>();
            
            using (var package = new ExcelPackage(new FileInfo(excelPath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                
                // Get the dimension (used range)
                var dimension = worksheet.Dimension;
                
                if (dimension == null)
                {
                    return Task.FromResult(columnLetters);
                }
                
                // Check each column in the used range
                for (int col = dimension.Start.Column; col <= dimension.End.Column; col++)
                {
                    bool hasData = false;
                    
                    // Check each row in this column
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
        
        public async Task<List<string>> ReadColumnByNumberAsync(string excelPath, int columnNumber)
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
                        texts.Add(cell.Value.ToString().Trim());
                    }
                }
            }
            
            if (texts.Count == 0)
            {
                throw new Exception($"No text found in column {GetColumnLetter(columnNumber)}.");
            }
            
            return texts;
        }
        
        public async Task<int> GetColumnNumberFromLetter(string columnLetter)
        {
            int result = 0;
            foreach (char c in columnLetter.ToUpper())
            {
                result = result * 26 + (c - 'A' + 1);
            }
            return result;
        }
    }
}