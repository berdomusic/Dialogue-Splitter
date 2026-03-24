using VO_Tool.Selectors;
using VO_Tool.Status;

namespace VO_Tool.UI
{
    public class UIControls
    {
        public Form MainForm { get; set; }
        public FileSelector ExcelSelector { get; set; }
        public FileSelector AudioSelector { get; set; }
        public Label Lbl_ExcelFile { get; set; }
        public Label Lbl_AudioFile { get; set; }
        public Label Lbl_VO_Text_Column { get; set; }
        public ComboBox Cmb_VO_Text_Column { get; set; }
        public Label Lbl_VO_Audio_Column { get; set; }
        public ComboBox Cmb_VO_Audio_Column { get; set; }
        public Button BtnProcess { get; set; }
        public StatusManager StatusManager { get; set; }
    }
}