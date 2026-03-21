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
        
        public void SetupAllTooltips(
            Form mainForm,
            FileSelector excelSelector,
            ComboBox cmbReaperProjects,
            FolderSelector outputFolderSelector,
            Label lbl_ExcelFile,
            Label lbl_ReaperProject,
            Label lbl_OutputFolder,
            Label lbl_VO_Text_Name,
            ComboBox cmb_VO_Text_Name,
            Label lbl_VO_Audio_Name,
            ComboBox cmb_VO_Audio_Name,
            Label lblSourceTrack,
            ComboBox cmbSourceTrack,
            Label lblOutputTrack,
            ComboBox cmbOutputTrack,
            Button btnProcess,
            StatusManager statusManager)
        {
            // Excel file selector - label and controls
            toolTip.SetToolTip(lbl_ExcelFile, "Select an Excel file containing VO text and audio file names");
            toolTip.SetToolTip(excelSelector.GetTextBox(), "Select an Excel file containing VO text and audio file names");
            toolTip.SetToolTip(excelSelector.GetBrowseButton(), "Browse for Excel file");
            
            // Reaper project dropdown
            toolTip.SetToolTip(lbl_ReaperProject, "Select an open Reaper project. Projects must be open in Reaper to appear here");
            toolTip.SetToolTip(cmbReaperProjects, "Select an open Reaper project. Projects must be open in Reaper to appear here");
            
            // Output folder selector - label and controls
            toolTip.SetToolTip(lbl_OutputFolder, "Choose where to save the split audio files");
            toolTip.SetToolTip(outputFolderSelector.GetTextBox(), "Choose where to save the split audio files");
            toolTip.SetToolTip(outputFolderSelector.GetBrowseButton(), "Browse for output folder");
            
            // VO Text Column
            toolTip.SetToolTip(lbl_VO_Text_Name, "Select the column containing the spoken text to match");
            toolTip.SetToolTip(cmb_VO_Text_Name, "Select the column containing the spoken text to match");
            
            // VO Audio File Name Column
            toolTip.SetToolTip(lbl_VO_Audio_Name, "Select the column containing the audio file names to export");
            toolTip.SetToolTip(cmb_VO_Audio_Name, "Select the column containing the audio file names to export");
            
            // Source Track
            toolTip.SetToolTip(lblSourceTrack, "Select the Reaper track that contains the source audio to split");
            toolTip.SetToolTip(cmbSourceTrack, "Select the Reaper track that contains the source audio to split");
    
            // Output Track
            toolTip.SetToolTip(lblOutputTrack, "Select the Reaper track where split audio segments will be placed");
            toolTip.SetToolTip(cmbOutputTrack, "Select the Reaper track where split audio segments will be placed");
            
            // Process button
            toolTip.SetToolTip(btnProcess, "Start processing: match text to audio and split files");
            
            // Status bar
            toolTip.SetToolTip(statusManager.GetStatusLabel(), "Current operation status");
        }
    }
}