using System;
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
using static HSR_SIM_GUI.StatCheck;
using static HSR_SIM_GUI.Utils;

namespace HSR_SIM_GUI
{
    public class ThreadWork
    {
        public static void DoWork(Object taskList)
        {
            RTaskList myTaskList = taskList as RTaskList;
            while (myTaskList.Tasks.Any(x => !x.Completed))
            {

                Thread.Sleep(100);
            }

        }
    }
    public partial class StatCheck : Form
    {
        public record RTask
        {
            public String ScenarioPath;
            public String ProfilePath;
            public bool Completed;
        }

        public record RTaskList
        {
            public List<RTask> Tasks;
        }

        public RTaskList MyTaskList;
        public StatCheck()
        {
            InitializeComponent();
            Utils.ApplyDarkLightTheme(this);

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
        private void StatCheck_Load(object sender, EventArgs e)
        {
            RefreshCbs();
            cbScenario.Text = IniF.IniReadValue("form", "Scenario");
            cbProfile.Text = IniF.IniReadValue("form", "Profile");
        }

        private void BtnGo_Click(object sender, EventArgs e)
        {
            BtnGo.Enabled = false;
            MyTaskList = new RTaskList();
            MyTaskList.Tasks = new List<RTask>();
            for (int i = 0; i < NmbIterations.Value; i++)
            {
                MyTaskList.Tasks.Add(new RTask()
                {
                    ScenarioPath = AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\" + cbScenario.Text,
                    ProfilePath = AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\" + cbProfile.Text
                });

            }
            PB1.Maximum = MyTaskList.Tasks.Count();
            Thread thread1 = new Thread(new ParameterizedThreadStart(ThreadWork.DoWork));
            thread1.Start(MyTaskList);


        }
    }
}
