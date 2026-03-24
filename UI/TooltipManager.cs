namespace VO_Tool.UI
{
    public class TooltipManager
    {
        private readonly ToolTip toolTip;
        
        public TooltipManager()
        {
            toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 100
            };
        }
        
        public void SetupAllTooltips(UIControls ui)
        {
            // Excel file selector
            toolTip.SetToolTip(ui.Lbl_ExcelFile, "Select an Excel file containing VO text and audio file names");
            toolTip.SetToolTip(ui.ExcelSelector.GetTextBox(), "Select an Excel file containing VO text and audio file names");
            toolTip.SetToolTip(ui.ExcelSelector.GetBrowseButton(), "Browse for Excel file");
    
            // Audio file selector
            toolTip.SetToolTip(ui.Lbl_AudioFile, "Select the source audio file (WAV, MP3, FLAC, M4A)");
            toolTip.SetToolTip(ui.AudioSelector.GetTextBox(), "Select the source audio file (WAV, MP3, FLAC, M4A)");
            toolTip.SetToolTip(ui.AudioSelector.GetBrowseButton(), "Browse for audio file");
    
            // VO Text Column
            toolTip.SetToolTip(ui.Lbl_VO_Text_Column, "Select the column letter (A, B, C, etc.) containing the spoken text");
            toolTip.SetToolTip(ui.Cmb_VO_Text_Column, "Select the column letter (A, B, C, etc.) containing the spoken text");
    
            // VO Audio File Name Column
            toolTip.SetToolTip(ui.Lbl_VO_Audio_Column, "Select the column letter (A, B, C, etc.) containing the output audio file names");
            toolTip.SetToolTip(ui.Cmb_VO_Audio_Column, "Select the column letter (A, B, C, etc.) containing the output audio file names");
    
            // Process button
            toolTip.SetToolTip(ui.BtnProcess, "Start processing: load Excel data");
    
            // Status bar
            toolTip.SetToolTip(ui.StatusManager.GetStatusLabel(), "Current operation status");
            
            toolTip.SetToolTip(ui.Tb_SimilarityThreshold, "Controls how aggressive Whisper merges words into segments.\n" +
                                                          "Lower = more segments (splits more easily)\n" +
                                                          "Higher = fewer segments (groups words together)\n" +
                                                          "Recommended: 0.3-0.7 (default: 0.5)");
        }
    }
}