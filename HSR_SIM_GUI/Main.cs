﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HSR_SIM_LIB;
using Ini;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using static HSR_SIM_LIB.Worker;
using static HSR_SIM_GUI.Utils;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_GUI
{

    public partial class Main : Form
    {

        private bool busy = false;
        readonly Worker wrk;
        private DebugWindow dbg;
        private readonly CallBacks.CallBackStr callBackStr;
        private readonly CallBacks.CallBackRender callBackRender;
        /// <summary>
        /// For text callback
        /// </summary>
        /// <param name="kv"></param> 
        public void WorkerCallBackString(KeyValuePair<string, string> kv)
        {
            if (string.Equals(kv.Key, Constant.MsgLog))
            {
                Utils.AddLine(LogWindow, kv.Value, 400);
                if (!busy)
                    LogWindow.ScrollToCaret();
            }
            else if (string.Equals(kv.Key, Constant.MsgDebug))
            {
                if (dbg != null && !dbg.IsDisposed)
                {
                    dbg.dbgText.AddLine(kv.Value);
                    if (!busy)
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


            callBackStr = new(WorkerCallBackString);
            callBackRender = new(WorkerCallBackImages);
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
            busy = true;
            wrk.CbRend -= callBackRender;
            wrk.MoveStep(false, 100);
            wrk.CbRend += callBackRender;
            busy = false;
            LogWindow.ScrollToCaret();
            wrk.DrawCombat();


        }

        private void Button3_Click_1(object sender, EventArgs e)
        {
            busy = true;
            wrk.CbRend -= callBackRender;
            wrk.MoveStep(true, -1);
            wrk.CbRend += callBackRender;
            busy = false;
            LogWindow.ScrollToCaret();
            wrk.DrawCombat();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            WarGear wg = new()
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            wg.Show();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            if (dbg == null || dbg.IsDisposed)
                dbg = new();
            dbg.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            wrk.Completed = false;
            wrk.MoveStep(false, 1, true);
        }

        private void BtnStatCheck_Click(object sender, EventArgs e)
        {
            StatCheck st = new()
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            st.Show();
        }

        private void cbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
