using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HSR_SIM_LIB;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_GUI.StatCheck;
using static HSR_SIM_GUI.Utils;


namespace HSR_SIM_GUI
{

    public partial class StatCheck : Form
    {
        public class ThreadWork
        {
            public static void ProcWork(Object task)
            {
                RTask myTask = task as RTask;
                Worker wrk = new Worker();

                wrk.LoadScenarioFromXml(myTask.ScenarioPath, myTask.ProfilePath);
                myTask.Result = wrk.GetCombatResult();
                myTask.Completed = true;
            }

            public static void DoWork(Object taskList)
            {
                List<Thread> myThreads = new List<Thread>();
                RTaskList myTaskList = taskList as RTaskList;
                for (int i = 0; i < myTaskList.ThreadCount; i++)
                {
                    Thread thd = new Thread(new ParameterizedThreadStart(ThreadWork.ProcWork));
                    myThreads.Add(thd);

                }
                while (myTaskList.Tasks.Any(x => !x.Fetched))
                {
                    foreach (Thread thr in (List<Thread>)myThreads.Where(x => x.IsAlive == false).ToList())
                    {
                        RTask task = myTaskList.Tasks.FirstOrDefault(x => !x.Started);
                        if (task != null)
                        {
                            task.Started = true;
                            Thread thThread = myThreads[myThreads.IndexOf(thr)];
                            int ndx = myThreads.IndexOf(thr);
                            myThreads[ndx] = null;
                            thThread = new Thread(new ParameterizedThreadStart(ThreadWork.ProcWork));
                            myThreads[ndx] = thThread;
                            thThread.Start(task);
                            myTaskList.PB.Invoke((MethodInvoker)delegate
                            {
                                // Running on the UI thread
                                myTaskList.PB.Value += 1;
                            });
                        }
                    }
                    //Aggregate data
                    foreach (RTask tsk in myTaskList.Tasks.Where(x => !x.Fetched && x.Completed))
                    {

                        myTaskList.Data.WinRate = (myTaskList.Data.WinRate + (tsk.Result.Success?1:0)*100)/ 2;
                        tsk.Fetched = true;
                    }
                    Thread.Sleep(100);
                }

                while (myThreads.Count(x => x.IsAlive == true) > 0)
                {
                    Thread.Sleep(100);
                }
                myTaskList.Btn.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    myTaskList.Btn.Enabled = true;
                });
                myTaskList.Report(myTaskList);
            }
        }

        public delegate void DLGreport(RTaskList taskList);
        private Thread mainThread;
        public record RTask
        {
            public string ScenarioPath;
            public string ProfilePath;
            public bool Started;
            public bool Completed;
            public bool Fetched;
            public Worker.RCombatResult Result;

        }



        public record RAggregatedData
        {
            public double WinRate = 0;
        }

        public record RTaskList
        {
            public List<RTask> Tasks;
            public int ThreadCount = 0;
            public ProgressBar PB;
            public Button Btn;
            public DLGreport Report;
            public RAggregatedData Data = new RAggregatedData();
        }

        public RTaskList MyTaskList;
        public StatCheck()
        {
            InitializeComponent();
            Utils.ApplyDarkLightTheme(this);

        }

        public void MyReport(RTaskList taskList)
        {
            lblWinRate.Invoke((MethodInvoker)delegate
            {
                // Running on the UI thread
                lblWinRate.Text = $"{ taskList.Data.WinRate:f}%";
            });

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
            mainThread = null;
        }

        private void BtnGo_Click(object sender, EventArgs e)
        {
            if (mainThread?.IsAlive ?? false)
                return;
            BtnGo.Enabled = false;
            mainThread = new Thread(new ParameterizedThreadStart(ThreadWork.DoWork));
            MyTaskList = new RTaskList();
            MyTaskList.Tasks = new List<RTask>();
            MyTaskList.ThreadCount = (int)NmbThreadsCount.Value;
            for (int i = 0; i < NmbIterations.Value; i++)
            {
                MyTaskList.Tasks.Add(new RTask()
                {
                    ScenarioPath = AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\" + cbScenario.Text,
                    ProfilePath = AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\" + cbProfile.Text
                });

            }

            PB1.Value = 0;
            PB1.Maximum = MyTaskList.Tasks.Count();
            MyTaskList.PB = PB1;
            MyTaskList.Btn = BtnGo;
            MyTaskList.Report = MyReport;
            mainThread.Start(MyTaskList);


        }

        private void lblWinRate_Click(object sender, EventArgs e)
        {

        }
    }
}
