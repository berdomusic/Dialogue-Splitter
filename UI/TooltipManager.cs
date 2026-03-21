using System.Windows.Forms;
using VO_Tool.Selectors;
using VO_Tool.Status;

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
            
            // Reaper project selector
            toolTip.SetToolTip(ui.Lbl_ReaperFile, "Select the Reaper project file (.rpp) that contains the source audio track");
            toolTip.SetToolTip(ui.ReaperSelector.GetTextBox(), "Select the Reaper project file (.rpp) that contains the source audio track");
            toolTip.SetToolTip(ui.ReaperSelector.GetBrowseButton(), "Browse for Reaper project file");
            
            // VO Text Column
            toolTip.SetToolTip(ui.Lbl_VO_Text_Column, "Select the column letter (A, B, C, etc.) containing the spoken text");
            toolTip.SetToolTip(ui.Cmb_VO_Text_Column, "Select the column letter (A, B, C, etc.) containing the spoken text");
            
            // VO Audio File Name Column
            toolTip.SetToolTip(ui.Lbl_VO_Audio_Column, "Select the column letter (A, B, C, etc.) containing the audio file names");
            toolTip.SetToolTip(ui.Cmb_VO_Audio_Column, "Select the column letter (A, B, C, etc.) containing the audio file names");
            
            // Source Track
            toolTip.SetToolTip(ui.LblSourceTrack, "Select the Reaper track that contains the source audio to split");
            toolTip.SetToolTip(ui.CmbSourceTrack, "Select the Reaper track that contains the source audio to split");
    
            // Output Track
            toolTip.SetToolTip(ui.LblOutputTrack, "Select the Reaper track where split audio segments will be placed");
            toolTip.SetToolTip(ui.CmbOutputTrack, "Select the Reaper track where split audio segments will be placed");
            
            // Process button
            toolTip.SetToolTip(ui.BtnProcess, "Start processing: match text to audio and split files");
            
            // Status bar
            toolTip.SetToolTip(ui.StatusManager.GetStatusLabel(), "Current operation status");
        }
    }
}