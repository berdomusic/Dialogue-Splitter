namespace VO_Tool.Selectors
{
    public class FileSelector
    {
        public TextBox GetTextBox() => txtFilePath;
        public Button GetBrowseButton() => btnBrowse;
        
        private TextBox txtFilePath = null!;
        private Button btnBrowse = null!;
        private Label label = null!;
        private OpenFileDialog openFileDialog = null!;
        private string fileFilter;
        
        public string FilePath => txtFilePath.Text;
        
        public FileSelector(string labelText, string fileFilter, int x, int y)
        {
            this.fileFilter = fileFilter;
            InitializeControls(labelText, x, y);
        }
        
        private void InitializeControls(string labelText, int x, int y)
        {
            label = new Label
            {
                Text = labelText,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(100, 25)
            };
            
            txtFilePath = new TextBox
            {
                Location = new System.Drawing.Point(x + 100, y - 3),
                Size = new System.Drawing.Size(330, 23),
                ReadOnly = true
            };
            
            btnBrowse = new Button
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(x + 440, y - 3),
                Size = new System.Drawing.Size(100, 30)
            };
            
            btnBrowse.Click += (s, e) => Browse_Click(s, e);
            
            openFileDialog = new OpenFileDialog
            {
                Filter = fileFilter,
                Title = $"Select {labelText.TrimEnd(':')}"
            };
        }
        
        public void SetFilePath(string path)
        {
            txtFilePath.Text = path;
            OnFileSelected?.Invoke(path);
        }
        
        private void Browse_Click(object? s, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog.FileName;
                OnFileSelected?.Invoke(openFileDialog.FileName);
            }
        }
        
        public event Action<string>? OnFileSelected;
        
        public void AddToForm(Form form)
        {
            form.Controls.Add(label);
            form.Controls.Add(txtFilePath);
            form.Controls.Add(btnBrowse);
        }
        
        public void Clear()
        {
            txtFilePath.Clear();
        }
    }
}