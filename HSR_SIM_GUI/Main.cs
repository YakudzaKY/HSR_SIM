using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HSR_SIM_LIB;
using Ini;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using static HSR_SIM_LIB.CallBacks;
using static HSR_SIM_LIB.Worker;

namespace HSR_SIM_GUI
{
    public partial class Main : Form
    {
        Worker wrk;
        IniFile ini = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "config.ini");


        /// <summary>
        /// For text callback
        /// </summary>
        /// <param name="kv"></param> 
        public void WorkerCallBackString(KeyValuePair<String, String> kv)
        {
            if (String.Equals(kv.Key, Constant.MsgLog))
            {
                Utils.AddLine(LogWindow, kv.Value, 200);
                LogWindow.ScrollToCaret();
            }
        }
        /// <summary>
        /// For imagesCallback
        /// </summary>
        /// <param name="kv"></param>
        public void WorkerCallBackImages(Bitmap combatImg)
        {
            if (combatImg != null)
            {
                combatOut.BackgroundImage?.Dispose();
                combatOut.BackgroundImage = new Bitmap(combatImg);
            }
        }
         void ApplyTheme(Color back, Color pan, Color btn, Color tbox, Color combox, Color textColor)
        {
            BackColor = back;

            foreach (Control item in Controls)
            {
                if (!(item is System.Windows.Forms.Label))
                {
                    item.BackColor = btn;
                    item.ForeColor = textColor;
                }
                else
                {
                    item.ForeColor = textColor;
                }
            
            }

           
        }

        public Main()
        {
            [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
            static extern bool ShouldSystemUseDarkMode();

            CallBackStr callBackStr = new CallBackStr(WorkerCallBackString);
            CallBackRender callBackRender = new CallBackRender(WorkerCallBackImages);
            InitializeComponent();
            wrk = new Worker();
            wrk.CbLog += callBackStr;
            wrk.CbRend += callBackRender;
            if (ShouldSystemUseDarkMode())            
                ApplyTheme(Utils.Zcolor(30, 30, 30), Utils.Zcolor(45, 45, 48), Utils.Zcolor(104, 104, 104), Utils.Zcolor(51, 51, 51), Color.Black, HSR_SIM_LIB.Constant.clrDefault);
            
            else
                ApplyTheme(Color.White, Utils.Zcolor(240, 240, 240), Utils.Zcolor(181, 181, 181), Utils.Zcolor(110, 110, 110), Color.White, Color.Black);
        }




        private void Main_Load(object sender, EventArgs e)
        {


            RefreshCbs();
            cbScenario.Text = ini.IniReadValue("form", "Scenario");
            cbProfile.Text = ini.IniReadValue("form", "Profile");
        }


        /// <summary>
        /// Load scenario click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            wrk.LoadScenarioFromXml(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\" + cbScenario.Text,
                AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\" + cbProfile.Text);
        }



        private void RefreshCbs()
        {
            cbScenario.Items.Clear();
            cbProfile.Items.Clear();
            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\");

            foreach (string file in files)
            {
                cbScenario.Items.Add(Path.GetFileName(file));
            }
            files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\");

            foreach (string file in files)
            {
                cbProfile.Items.Add(Path.GetFileName(file));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            wrk.MoveStep();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            wrk.MoveStep(true);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            RefreshCbs();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {

            wrk.MoveStep(false, -1);


        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            wrk.MoveStep(true, -1);
        }
    }
}
