using System;
using System.Windows.Forms;

namespace HSR_SIM_GUI;

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