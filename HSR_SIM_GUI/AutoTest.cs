using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HSR_SIM_GUI.ChartTools;
using HSR_SIM_GUI.TaskTools;
using HSR_SIM_GUI.ThreadTools;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Utils;
using Newtonsoft.Json;
using static HSR_SIM_GUI.GuiUtils;

namespace HSR_SIM_GUI
{
    public partial class AutoTest : Form
    {
        private readonly string testFolder = AppDomain.CurrentDomain.BaseDirectory + "DATA\\AutoTest\\";
        public AutoTest()
        {
            InitializeComponent();
            ApplyDarkLightTheme(this);
        }

        private void RefreshTest()
        {
            dgTest.Rows.Clear();

            var files = Directory.GetFiles(testFolder, "*.xml");
            dgTest.Rows.Add(files.Count());
            int i = 0;
            foreach (var file in files)
            {
                dgTest.Rows[i].Cells[0].Value = Path.GetFileName(file);
                i++;
            }


        }
        private void AutoTest_Load(object sender, EventArgs e)
        {
            RefreshTest();
        }

        /// <summary>
        /// Go check all tests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoBtn_Click(object sender, EventArgs e)
        {
            GoBtn.Enabled = false;
            
            List<SimTask> myTaskList = new List<SimTask>();
      


            //generate thread task
            for (int i = 0; i < dgTest.Rows.Count; i++)
            {
                myTaskList.Add(new SimTask
                {
                    SimScenario =  XMLLoader.LoadCombatFromXml(testFolder + dgTest.Rows[i].Cells[0].Value,null),
                    DevLogPath = DevModeUtils.GetDevLogPath(testFolder + dgTest.Rows[i].Cells[0].Value,null),
                    DevMode=true

                });
            }
            ThreadJob thdJob = new ThreadJob(myTaskList, 10);//all iterations should be same
            AggregateThread mainThread = new AggregateThread(thdJob,8);
            //start
            mainThread.Start();

            while (mainThread.IsAlive)
            {
                Thread.Sleep(100);
    
            }

            int successTestCount = 0;
            //fetch result and compare with json file
            /*
            for (int i = 0; i < dgTest.Rows.Count; i++)
            {
                /string jsonFile = File.ReadAllText(myTaskList.Tasks[i].Scenario + ".json");
                //null some fields
                string jsonRes = JsonConvert.SerializeObject(myTaskList.Tasks[i].Data);
                bool successTest = String.Equals(jsonFile, jsonRes);
                dgTest.Rows[i].Cells[1].Value=successTest?"Ok":"FALSE";
                if (successTest)
                    successTestCount++;

            }*/

            lblRes.Text = $"{successTestCount} of {dgTest.Rows.Count} success";
            GoBtn.Enabled = true;
        }
    }
}
