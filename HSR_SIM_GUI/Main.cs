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
        readonly Worker wrk;
        private DebugWindow dbg;

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
            else if (String.Equals(kv.Key, Constant.MsgDebug))
            {
                if (dbg!=null&&!dbg.IsDisposed)
                {
                    dbg.dbgText.AddLine(kv.Value);
                    LogWindow.ScrollToCaret();
                }
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

            
            CallBackStr callBackStr = new(WorkerCallBackString);
            CallBackRender callBackRender = new(WorkerCallBackImages);
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
        private void Button1_Click(object sender, EventArgs e)
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

        private void Button3_Click(object sender, EventArgs e)
        {
            wrk.MoveStep();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            wrk.MoveStep(true);
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            RefreshCbs();
        }

        private void Button2_Click_1(object sender, EventArgs e)
        {

            wrk.MoveStep(false, -1);


        }

        private void Button3_Click_1(object sender, EventArgs e)
        {
            wrk.MoveStep(true, -1);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            WarGear wg = new()
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            wg.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (dbg ==null || dbg.IsDisposed)
                dbg= new();
            dbg.Show();
        }
    }
}
