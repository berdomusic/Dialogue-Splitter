using System.Windows.Forms;
using VO_Tool.UI;

namespace VO_Tool
{
    public class Main : Form
    {
        private readonly FormManager formManager;
        
        public Main()
        {
            formManager = new FormManager(this);
        }
    }
}