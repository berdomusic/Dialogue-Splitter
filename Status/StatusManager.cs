using VO_Tool.Services;

namespace VO_Tool.Status
{
    public class StatusManager(int x, int y, int width, int height)
    {
        public Label GetStatusLabel() => lblStatus;
        
        private readonly Label lblStatus = new()
        {
            Text = "Ready",
            Location = new Point(x, y),
            Size = new Size(width, height),
            TextAlign = ContentAlignment.MiddleLeft,
            BorderStyle = BorderStyle.FixedSingle
        };
        private readonly bool loggingEnabled = true;

        public void UpdateStatus(string message, bool addToLog = true)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => UpdateStatus(message, addToLog)));
                return;
            }
            
            lblStatus.Text = message;
            
            if (addToLog && loggingEnabled)
            {
                LogService.AddMessage(message);
            }
        }
        
        public void AddToForm(Form form)
        {
            form.Controls.Add(lblStatus);
        }
        
        public void ClearLog()
        {
            LogService.ClearMessages();
        }
        
        public List<string> GetLogMessages()
        {
            return LogService.GetMessages();
        }
    }
}