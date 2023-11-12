using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HSR_SIM_GUI
{
    public partial class DebugWindow : Form
    {
        public DebugWindow()
        {
            InitializeComponent();
            GuiUtils.ApplyDarkLightTheme(this);
        }

        private void dbgText_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
