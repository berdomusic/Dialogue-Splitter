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

            if (string.IsNullOrEmpty(ui.ReaperSelector.FilePath))
            {
                errorMessage = "Please select a Reaper project file.";
                return false;
            }

            if (!File.Exists(ui.ExcelSelector.FilePath))
            {
                errorMessage = "Excel file does not exist.";
                return false;
            }

            if (!File.Exists(ui.ReaperSelector.FilePath))
            {
                errorMessage = "Reaper project file does not exist.";
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

            if (ui.CmbSourceTrack.SelectedItem == null)
            {
                errorMessage = "Please select a source track.";
                return false;
            }

            if (ui.CmbOutputTrack.SelectedItem == null)
            {
                errorMessage = "Please select an output track.";
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
        
        // ============ Drag and Drop ============
        public static void SetupFileDrop(Control control, 
            Action<string> onExcelFile, 
            Action<string> onReaperFile,
            Action<string>? onOtherFile = null)
        {
            control.AllowDrop = true;
            control.DragEnter += OnDragEnter!;
            control.DragDrop += (s, e) => OnFileDrop(s, e, onExcelFile, onReaperFile, onOtherFile);
        }
        
        private static void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        
        private static void OnFileDrop(object? sender, DragEventArgs e, 
            Action<string> onExcelFile, 
            Action<string> onReaperFile,
            Action<string>? onOtherFile)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is not string[] files) return;
            
            foreach (string file in files)
            {
                if (IsExcelFile(file))
                {
                    onExcelFile(file);
                }
                else if (IsReaperFile(file))
                {
                    onReaperFile(file);
                }
                else
                {
                    onOtherFile?.Invoke(file);
                }
            }
        }
        
        // ============ File Type Detection ============
        public static bool IsExcelFile(string filePath)
        {
            return filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || 
                   filePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase);
        }
        
        public static bool IsReaperFile(string filePath)
        {
            return filePath.EndsWith(".rpp", StringComparison.OrdinalIgnoreCase);
        }
        
        public static string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }
        
        // ============ Control Creation Helpers ============
        public static Label CreateLabel(string text, int x, int y, int width = 130, int height = 25)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height)
            };
        }
        
        public static ComboBox CreateComboBox(int x, int y, int width = 200, int height = 23, bool enabled = true)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = enabled
            };
        }
        
        public static Button CreateButton(string text, int x, int y, int width = 100, int height = 35, Color? backColor = null)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = backColor ?? Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
        }
        
        // ============ Labeled Control Helpers ============
        public static (Label label, ComboBox comboBox) CreateLabeledComboBox(
            string labelText, 
            int x, 
            int y, 
            int labelWidth = 130, 
            int comboBoxWidth = 200, 
            bool enabled = false)
        {
            var label = CreateLabel(labelText, x, y, labelWidth);
            var comboBox = CreateComboBox(x + labelWidth + 10, y - 3, comboBoxWidth, enabled: enabled);
            
            return (label, comboBox);
        }
        
        // ============ Control Management ============
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
        
        // ============ ComboBox Helpers ============
        public static void PopulateComboBox<T>(ComboBox comboBox, IEnumerable<T> items, bool selectFirst = true)
        {
            comboBox.Items.Clear();
            foreach (var item in items)
            {
                if (item != null)
                {
                    comboBox.Items.Add(item);
                }
            }
            if (selectFirst && comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }
        
        // ============ Form Helpers ============
        public static void CenterForm(Form form)
        {
            form.StartPosition = FormStartPosition.CenterScreen;
        }
        
        public static void SetFormSize(Form form, int width, int height)
        {
            form.ClientSize = new Size(width, height);
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
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