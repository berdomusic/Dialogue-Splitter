using VO_Tool.Selectors;
using VO_Tool.Status;

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
        
        public StatusManager AddStatusBar(int x = 20, int width = 560, int height = 30)
        {
            var status = new StatusManager(x, currentY, width, height);
            status.AddToForm(form);
            
            // Add 50 units of space below status bar
            int bottomPadding = 50;
            int formHeight = currentY + height + bottomPadding;
            int formWidth = maxWidth + 40;
            
            form.ClientSize = new Size(formWidth, formHeight);
            
            return status;
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