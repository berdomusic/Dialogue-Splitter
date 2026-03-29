namespace VO_Tool.UI
{
    public class AboutDialog : Form
    {
        public AboutDialog()
        {
            Text = "About Dialogue Splitter";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(400, 250);

            var lblTitle = new Label
            {
                Text = "Dialogue Splitter",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(360, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblCopyright = new Label
            {
                Text = "Berdo Music - Michal Cywinski",
                Location = new Point(20, 95),
                Size = new Size(360, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblDiscord = new Label
            {
                Text = "Discord: mcyw.",
                Location = new Point(20, 130),
                Size = new Size(360, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblEmail = new Label
            {
                Text = "Email: kontakt@berdo-music.pl",
                Location = new Point(20, 160),
                Size = new Size(360, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(150, 200),
                Size = new Size(100, 35),
                DialogResult = DialogResult.OK
            };

            Controls.Add(lblTitle);
            Controls.Add(lblCopyright);
            Controls.Add(lblDiscord);
            Controls.Add(lblEmail);
            Controls.Add(btnOk);
        }
    }
}