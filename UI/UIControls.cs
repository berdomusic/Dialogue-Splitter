using System.Windows.Forms;
using VO_Tool.Selectors;
using VO_Tool.Status;

namespace VO_Tool.UI
{
    public class UIControls
    {
        public Form MainForm { get; set; }
        public FileSelector ExcelSelector { get; set; }
        public FileSelector ReaperSelector { get; set; }
        public Label Lbl_ExcelFile { get; set; }
        public Label Lbl_ReaperFile { get; set; }
        public Label Lbl_VO_Text_Column { get; set; }
        public ComboBox Cmb_VO_Text_Column { get; set; }
        public Label Lbl_VO_Audio_Column { get; set; }
        public ComboBox Cmb_VO_Audio_Column { get; set; }
        public Label LblSourceTrack { get; set; }
        public ComboBox CmbSourceTrack { get; set; }
        public Label LblOutputTrack { get; set; }
        public ComboBox CmbOutputTrack { get; set; }
        public Button BtnProcess { get; set; }
        public StatusManager StatusManager { get; set; }
    }
}