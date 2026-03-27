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
            
            // Output folder selector
            toolTip.SetToolTip(ui.Lbl_OutputFolder, "Choose where to save the split audio files");
            toolTip.SetToolTip(ui.OutputFolderSelector.GetTextBox(), "Choose where to save the split audio files");
            toolTip.SetToolTip(ui.OutputFolderSelector.GetBrowseButton(), "Browse for output folder");
    
            // VO Text Column
            toolTip.SetToolTip(ui.Lbl_VO_Text_Column, "Select the column letter (A, B, C, etc.) containing the spoken text");
            toolTip.SetToolTip(ui.Cmb_VO_Text_Column, "Select the column letter (A, B, C, etc.) containing the spoken text");
    
            // VO Audio File Name Column
            toolTip.SetToolTip(ui.Lbl_VO_Audio_Column, "Select the column letter (A, B, C, etc.) containing the output audio file names");
            toolTip.SetToolTip(ui.Cmb_VO_Audio_Column, "Select the column letter (A, B, C, etc.) containing the output audio file names");
            
            toolTip.SetToolTip(ui.Lbl_Model, "Select installed Whisper model");
            toolTip.SetToolTip(ui.Cmb_Model, "Select installed Whisper model");
            
            toolTip.SetToolTip(ui.Lbl_Language, "Select the language of the audio for better accuracy");
            toolTip.SetToolTip(ui.Cmb_Language, "Select the language of the audio for better accuracy");
    
            // Process button
            toolTip.SetToolTip(ui.BtnProcess, "Start processing: load Excel data");
    
            // Status bar
            toolTip.SetToolTip(ui.StatusManager.GetStatusLabel(), "Current operation status");
            
            toolTip.SetToolTip(ui.Tb_SimilarityThreshold, 
                "Minimum similarity score required to consider a transcribed segment a match to the expected text.\n\n" +
                "Lower = more matches (may include incorrect matches)\n" +
                "Higher = fewer matches (only exact or very close matches)\n\n" +
                "How similarity is calculated:\n" +
                "• 100% - Exact match\n" +
                "• 95% - Contains the text\n" +
                "• 70-90% - Levenshtein ratio + word overlap\n\n" +
                "Recommended: 70-85% ");
        }
    }
}