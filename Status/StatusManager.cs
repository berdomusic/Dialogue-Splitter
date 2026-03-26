using VO_Tool.Services;
using System.Drawing;

namespace VO_Tool.Status
{
    public class StatusManager
    {
        public Label GetStatusLabel() => lblStatus;
        
        private Label lblStatus = null!;
        private bool _loggingEnabled = true;
        
        public StatusManager(int x, int y, int width, int height)
        {
            lblStatus = new Label
            {
                Text = "Ready",
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, height),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle
            };
        }
        
        public void UpdateStatus(string message, bool addToLog = true)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => UpdateStatus(message, addToLog)));
                return;
            }
            
            lblStatus.Text = message;
            
            if (addToLog && _loggingEnabled)
            {
                LogService.AddMessage(message);
            }
        }
        
        public void UpdatePosition(int y)
        {
            lblStatus.Location = new System.Drawing.Point(lblStatus.Location.X, y);
        }
        
        public void AddToForm(Form form)
        {
            form.Controls.Add(lblStatus);
        }
        
        public void SetLoggingEnabled(bool enabled)
        {
            _loggingEnabled = enabled;
            if (!enabled)
            {
                LogService.ClearMessages();
            }
        }
        
        public void ClearLog()
        {
            LogService.ClearMessages();
        }
        
        public void SaveLogToFile(string outputFolder, string audioFile, string excelFile, string textColumn, string audioColumn, WhisperModel model, WhisperLanguage language)
        {
            LogService.SaveLogToFile(outputFolder, audioFile, excelFile, textColumn, audioColumn, model, language);
        }
    }
}