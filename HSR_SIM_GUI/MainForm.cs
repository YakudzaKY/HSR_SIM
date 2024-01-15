using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static HSR_SIM_GUI.GuiUtils;

namespace HSR_SIM_GUI
{
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
    }
}
