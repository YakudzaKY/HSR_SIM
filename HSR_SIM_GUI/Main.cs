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
using static HSR_SIM_GUI.Utils;

namespace HSR_SIM_GUI
{
    public partial class Main : Form
    {
        Worker wrk;


        /// <summary>
        /// For text callback
        /// </summary>
        /// <param name="kv"></param> 
        public void WorkerCallBackString(KeyValuePair<String, String> kv)
        {
            if (String.Equals(kv.Key, Constant.MsgLog))
            {
                Utils.AddLine(LogWindow, kv.Value, 400);
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


        public Main()
        {
            


            CallBackStr callBackStr = new CallBackStr(WorkerCallBackString);
            CallBackRender callBackRender = new CallBackRender(WorkerCallBackImages);
            InitializeComponent();
            wrk = new Worker();
            wrk.CbLog += callBackStr;
            wrk.CbRend += callBackRender;
            Utils.ApplyDarkLightTheme(this);
        }




        private void Main_Load(object sender, EventArgs e)
        {


            RefreshCbs();
            cbScenario.Text = IniF.IniReadValue("form", "Scenario");
            cbProfile.Text = IniF.IniReadValue("form", "Profile");
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

        private void button4_Click(object sender, EventArgs e)
        {
            WarGear wg = new WarGear();
            wg.StartPosition = FormStartPosition.CenterScreen;
            wg.Show();
        }
    }
}
