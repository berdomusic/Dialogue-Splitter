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
            
            // Log file checkbox
            toolTip.SetToolTip(ui.ChkCreateLogFile, 
                "Create a detailed log file with all status messages, timestamps, and processing information.\n\n" +
                "The log is saved in a folder named: log_YYYYMMDD_HHMMSS_model_language/\n" +
                "File name: split_log_YYYYMMDD_HHMMSS_model_language.txt");
            
            // CSV file checkbox
            toolTip.SetToolTip(ui.ChkCreateCsvFile, 
                "Create a CSV file with all matched segments that passed the similarity threshold.\n\n" +
                "Columns: Source Audio File, Start, End, Audio File Name, Expected Text, Transcribed Text, Similarity\n\n" +
                "The CSV is saved in the same folder as the log file.\n" +
                "File name: matches_YYYYMMDD_HHMMSS.csv");
            
            // Start Padding tooltip
            toolTip.SetToolTip(ui.LblStartPadding, 
                "Adjust the start time of each segment in the CSV export.\n\n" +
                "Positive values add time before the detected segment.\n" +
                "Negative values cut time from the beginning of the segment.\n\n" +
                "Range: -2.00 to +2.00 seconds\n" +
                "Default: 0.20 seconds");
            toolTip.SetToolTip(ui.NudStartPadding, 
                "Adjust the start time of each segment in the CSV export.\n\n" +
                "Positive values add time before the detected segment.\n" +
                "Negative values cut time from the beginning of the segment.\n\n" +
                "Range: -2.00 to +2.00 seconds\n");
            
            // End Padding tooltip
            toolTip.SetToolTip(ui.LblEndPadding, 
                "Adjust the end time of each segment in the CSV export.\n\n" +
                "Positive values add time after the detected segment.\n" +
                "Negative values cut time from the end of the segment.\n\n" +
                "Range: -2.00 to +2.00 seconds\n");
            toolTip.SetToolTip(ui.NudEndPadding, 
                "Adjust the end time of each segment in the CSV export.\n\n" +
                "Positive values add time after the detected segment.\n" +
                "Negative values cut time from the end of the segment.\n\n" +
                "Range: -2.00 to +2.00 seconds\n" +
                "Default: 0.50 seconds");
        }
    }
}