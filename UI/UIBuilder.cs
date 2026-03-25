using VO_Tool.Selectors;
using VO_Tool.Status;
using VO_Tool.UI;

namespace VO_Tool
{
    public class UIBuilder
    {
        private Form form;
        private int currentY;
        private int maxWidth;
        
        public UIBuilder(Form form, int startY = 30)
        {
            this.form = form;
            this.currentY = startY;
            this.maxWidth = 0;
            SetupFormProperties();
        }
        
        private void SetupFormProperties()
        {
            form.Text = "VO Splitter";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
        }
        
        public void SetFormSize(int width, int height)
        {
            form.ClientSize = new Size(width, height);
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
        }
        
        public void CenterForm()
        {
            form.StartPosition = FormStartPosition.CenterScreen;
        }
        
        public (Label label, ComboBox comboBox) CreateLabeledComboBox(
            string labelText, 
            int x, 
            int y, 
            int labelWidth = 130, 
            int comboBoxWidth = 200, 
            bool enabled = false)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                Size = new Size(labelWidth, 25)
            };
            
            var comboBox = new ComboBox
            {
                Location = new Point(x + labelWidth + 10, y - 3),
                Size = new Size(comboBoxWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = enabled
            };
            
            return (label, comboBox);
        }
        
        public (Label label, FileSelector selector) AddFileSelectorWithLabel(string labelText, string fileFilter, int x = 20)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, currentY),
                Size = new Size(120, 25)
            };
            form.Controls.Add(label);
    
            var selector = new FileSelector(labelText, fileFilter, x, currentY);
            selector.AddToForm(form);
            currentY += 50;
            UpdateMaxWidth(x + 550);
    
            return (label, selector);
        }
        
        public (Label label, FolderSelector selector) AddFolderSelectorWithLabel(string labelText, int x = 20)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, currentY),
                Size = new Size(120, 25)
            };
            form.Controls.Add(label);

            var selector = new FolderSelector(labelText, x, currentY);
            selector.AddToForm(form);
            currentY += 50;
            UpdateMaxWidth(x + 550);

            return (label, selector);
        }
        
        public Button AddButton(string text, EventHandler clickHandler, int x = 250, int width = 100, int height = 35)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, currentY + 10),
                Size = new Size(width, height),
                BackColor = Color.LightBlue
            };
            button.Click += clickHandler;
            form.Controls.Add(button);
            currentY += 50;
            UpdateMaxWidth(x + width);
            
            return button;
        }
        
        public TrackBar AddSimilaritySlider(int x = 20, int min = 10, int max = 100, int defaultValue = 75)
        {
            var label = new Label
            {
                Text = "Similarity Threshold:",
                Location = new Point(x, currentY),
                Size = new Size(130, 25)
            };
            form.Controls.Add(label);
            
            var trackBar = new TrackBar
            {
                Location = new Point(x + 140, currentY - 5),
                Size = new Size(200, 45),
                Minimum = min,
                Maximum = max,
                Value = defaultValue,
                TickFrequency = 10,
                TickStyle = TickStyle.BottomRight
            };
            form.Controls.Add(trackBar);
            
            var valueLabel = new Label
            {
                Text = $"{defaultValue / 100.0:F2}",
                Location = new Point(x + 350, currentY),
                Size = new Size(50, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            form.Controls.Add(valueLabel);
            
            trackBar.Scroll += (s, e) =>
            {
                valueLabel.Text = $"{trackBar.Value / 100.0:F2}";
            };
            
            currentY += 45;
            UpdateMaxWidth(x + 400);
            
            return trackBar;
        }
        
        public StatusManager AddStatusBar(int x = 20, int width = 560, int height = 30)
        {
            var status = new StatusManager(x, currentY, width, height);
            status.AddToForm(form);
            
            int bottomPadding = 50;
            int formHeight = currentY + height + bottomPadding;
            int formWidth = maxWidth + 40;
            
            form.ClientSize = new Size(formWidth, formHeight);
            
            return status;
        }
        
        public void SetupFileDrop(Control control, 
            Action<string> onExcelFile, 
            Action<string> onAudioFile,
            Action<string>? onOtherFile = null)
        {
            control.AllowDrop = true;
            control.DragEnter += (s, e) => UIControls.OnDragEnter(s, e);
            control.DragDrop += (s, e) => UIControls.OnFileDrop(s, e, onExcelFile, onAudioFile, onOtherFile);
        }
        
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
        
        public void UpdateCurrentY(int newY)
        {
            currentY = newY;
        }
        
        private void UpdateMaxWidth(int width)
        {
            if (width > maxWidth)
                maxWidth = width;
        }
        
        public int GetCurrentY() => currentY;
    }
}