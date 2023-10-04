﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HSR_SIM_LIB;
using static HSR_SIM_LIB.CallBacks;
using static HSR_SIM_LIB.Worker;

namespace HSR_SIM_GUI
{
    public partial class Main : Form
    {
        Worker wrk;        

        public  void WorkerCallBackString(KeyValuePair<String, String> kv)
        {
            if (String.Equals(kv.Key, Constant.MsgLog))
            {
                LogWindow.AppendText(kv.Value+ "\r\n"  );
                LogWindow.ScrollToCaret();
            }
        }

        public Main()
        {
            CallBackStr callBackStr = new CallBackStr(WorkerCallBackString);
            InitializeComponent();
            wrk = new Worker();
            wrk.CbLog += callBackStr;
            wrk.Init();

        }

        private void Main_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Load scenario click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory+ "DATA\\Scenario\\";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                wrk.LoadScenarioFromXml(openFileDialog1.FileName);
            }
        }
    }
}
