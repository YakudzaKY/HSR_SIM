using System;
using System.Windows.Forms;
using static HSR_SIM_GUI.GuiUtils;

namespace HSR_SIM_GUI;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        ApplyDarkLightTheme(this);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        GUIForm form = new()
        {
            StartPosition = FormStartPosition.CenterScreen
        };
        form.Show();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        CharImport form = new()
        {
            StartPosition = FormStartPosition.CenterScreen
        };
        form.Show();
    }

    private void button3_Click(object sender, EventArgs e)
    {
        StatCheck form = new()
        {
            StartPosition = FormStartPosition.CenterScreen
        };
        form.Show();
    }

    private void button4_Click(object sender, EventArgs e)
    {
        AutoTest form = new()
        {
            StartPosition = FormStartPosition.CenterScreen
        };
        form.Show();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
    }
}