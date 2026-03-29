namespace VO_Tool.Selectors
{
    public class FolderSelector
    {
        public TextBox GetTextBox() => txtFolderPath;
        public Button GetBrowseButton() => btnBrowse;
        
        private TextBox txtFolderPath = null!;
        private Button btnBrowse = null!;
        private Label label = null!;
        private FolderBrowserDialog folderBrowserDialog = null!;
        
        public string FolderPath => txtFolderPath.Text;
        
        public FolderSelector(string labelText, int x, int y)
        {
            InitializeControls(labelText, x, y);
        }
        
        private void InitializeControls(string labelText, int x, int y)
        {
            label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                Size = new Size(100, 25)
            };
            
            txtFolderPath = new TextBox
            {
                Location = new Point(x + 100, y - 3),
                Size = new Size(330, 23),
                ReadOnly = true
            };
            
            btnBrowse = new Button
            {
                Text = "Browse...",
                Location = new Point(x + 440, y - 3),
                Size = new Size(100, 30)
            };
            
            btnBrowse.Click += (s, e) => Browse_Click(s, e);
            
            folderBrowserDialog = new FolderBrowserDialog
            {
                Description = $"Select {labelText.TrimEnd(':')}",
                ShowNewFolderButton = true
            };
        }
        
        public void SetFolderPath(string path)
        {
            txtFolderPath.Text = path;
            OnFolderSelected?.Invoke(path);
        }
        
        private void Browse_Click(object? s, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = folderBrowserDialog.SelectedPath;
                OnFolderSelected?.Invoke(folderBrowserDialog.SelectedPath);
            }
        }
        
        public event Action<string>? OnFolderSelected;
        
        public void AddToForm(Form form)
        {
            form.Controls.Add(label);
            form.Controls.Add(txtFolderPath);
            form.Controls.Add(btnBrowse);
        }
        
        public void Clear()
        {
            txtFolderPath.Clear();
        }
    }
}