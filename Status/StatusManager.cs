namespace VO_Tool.Status
{
    public class StatusManager
    {
        public Label GetStatusLabel() => lblStatus;
        
        private Label lblStatus = null!;
        
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
        
        public void UpdateStatus(string message)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => UpdateStatus(message)));
                return;
            }
            
            lblStatus.Text = message;
        }
        
        public void UpdatePosition(int y)
        {
            lblStatus.Location = new System.Drawing.Point(lblStatus.Location.X, y);
        }
        
        public void AddToForm(Form form)
        {
            form.Controls.Add(lblStatus);
        }
    }
}