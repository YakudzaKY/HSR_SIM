using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
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
using System.Windows.Forms.DataVisualization.Charting;


namespace HSR_SIM_GUI
{

    public partial class StatCheck : Form
    {
        public class ThreadWork
        {
            public static void ProcWork(Object links)
            {
                RLinksToOjbects rLinks = links as RLinksToOjbects;


                Worker wrk = new Worker();

                wrk.LoadScenarioFromXml(rLinks.Task.ScenarioPath, rLinks.Task.ProfilePath);
                wrk.GetCombatResult(rLinks.Result);

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
                    //queue tasks
                    foreach (Thread thr in (List<Thread>)myThreads.Where(x => x.IsAlive == false).ToList())
                    {
                        RTask task = myTaskList.Tasks.FirstOrDefault(x => x.Results != null && x.Results.Count() < x.Iterations);

                        if (task != null)
                        {
                            Worker.RCombatResult result = new Worker.RCombatResult();
                            task.Results.Add(result);
                            Thread thThread = myThreads[myThreads.IndexOf(thr)];
                            int ndx = myThreads.IndexOf(thr);
                            myThreads[ndx] = null;
                            thThread = new Thread(new ParameterizedThreadStart(ThreadWork.ProcWork));
                            myThreads[ndx] = thThread;
                            RLinksToOjbects links = new RLinksToOjbects();
                            links.Task = task;
                            links.Result = result;
                            thThread.Start(links);
                        }
                    }
                    //fetch data
                    foreach (RTask fetchTask in myTaskList.Tasks.Where(x => !x.Fetched && x.Results.Count(y => y.Success != null) == x.Iterations))
                    {
                        fetchTask.Fetched = true;
                        fetchTask.Data.WinRate = fetchTask.Results.Average(x => x.Success ?? false ? 100 : 0);
                        fetchTask.Data.TotalAV = fetchTask.Results.Average(x => x.TotalAv);
                        fetchTask.Data.Cycles = fetchTask.Results.Average(x => x.Cycles);
                        fetchTask.Data.avgDPAV = fetchTask.Results.Average(x => x.Combatants
                            .Sum(z =>
                                (z.Damages.Sum(x => x.Value) / x.TotalAv
                                )));
                        fetchTask.Data.minDPAV = fetchTask.Results.Min(x => x.Combatants
                            .Sum(z =>
                                (z.Damages.Sum(x => x.Value) / x.TotalAv
                                )));
                        fetchTask.Data.maxDPAV = fetchTask.Results.Max(x => x.Combatants
                            .Sum(z =>
                                (z.Damages.Sum(x => x.Value) / x.TotalAv
                                )));
                        if (fetchTask.Results.Count(x => x.Success ?? false) > 0)
                            foreach (string unit in fetchTask.Results.First(x => x.Success ?? false)
                                         .Combatants
                                         .Select(y => y.CombatUnit))
                            {
                                PartyUnit prUnit = new PartyUnit();
                                prUnit.CombatUnit = unit;

                                prUnit.avgDPAV = fetchTask.Results.Average(x => x.Combatants
                                    .Where(y => y.CombatUnit == unit).Sum(z =>
                                        (z.Damages.Sum(x => x.Value) / x.TotalAv
                                        )));
                                prUnit.minDPAV = fetchTask.Results.Min(x => x.Combatants.Where(y => y.CombatUnit == unit).Sum(z =>
                                        (z.Damages.Sum(x => x.Value) / x.TotalAv
                                    )));
                                prUnit.maxDPAV = fetchTask.Results.Max(x => x.Combatants.Where(y => y.CombatUnit == unit)
                                    .Sum(z =>
                                        (z.Damages.Sum(x => x.Value) / x.TotalAv
                                        )));

                                Type[] typeArray = new Type[] { typeof(DirectDamage), typeof(DoTDamage), typeof(BreakShieldDoTDamage), typeof(ShieldBreak) };

                                foreach (Type typ in typeArray)
                                {

                                    prUnit.avgByTypeDPAV.Add(typ, fetchTask.Results.Average(x => x.Combatants.Where(y => y.CombatUnit == unit).
                                        Sum(z =>
                                            z.Damages[typ] / x.TotalAv
                                        )));

                                }
                                fetchTask.Data.PartyUnits.Add(prUnit);

                            }
                        //clear resources
                        foreach (var sd in fetchTask.Results)
                        {
                            sd.Combatants = null;
                        }
                        System.GC.Collect();
                    }



                    Thread.Sleep(10);
                }

                while (myThreads.Count(x => x.IsAlive == true) > 0)
                {
                    Thread.Sleep(100);
                }

                System.GC.Collect();
            }
        }

        public delegate void DLGreport(RTaskList taskList);
        private Thread mainThread;
        public record RTask
        {
            public string ScenarioPath;
            public string ProfilePath;
            public List<Worker.RCombatResult> Results = new List<Worker.RCombatResult>();
            public bool Fetched;
            public RAggregatedData Data = new RAggregatedData();
            public int Iterations { get; set; }
        }

        public record RLinksToOjbects
        {
            public RTask Task;
            public Worker.RCombatResult Result;
        }

        public record PartyUnit
        {
            public string CombatUnit;
            public Dictionary<Type, double> avgByTypeDPAV = new Dictionary<Type, double>();
            public double avgDPAV;
            public double maxDPAV;
            public double minDPAV;
        }

        public record RAggregatedData
        {
            public double WinRate { get; set; }
            public double TotalAV { get; set; }
            public double Cycles { get; set; }
            public double avgDPAV;
            public double maxDPAV;
            public double minDPAV;

            public List<PartyUnit> PartyUnits = new List<PartyUnit>();
        }

        public record RTaskList
        {
            public List<RTask> Tasks;
            public int ThreadCount = 0;

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
            NmbThreadsCount.Value = Environment.ProcessorCount - 2;
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
            MyTaskList.Tasks.Add(new RTask()
            {
                ScenarioPath = AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\" + cbScenario.Text,
                ProfilePath = AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\" + cbProfile.Text,
                Iterations = (int)NmbIterations.Value
            });




            PB1.Value = 0;
            PB1.Maximum = MyTaskList.Tasks.Sum(x => x.Iterations);
            mainThread.Start(MyTaskList);

            while (mainThread.IsAlive)
            {
                Thread.Sleep(100);
                PB1.Value = MyTaskList.Tasks.Sum(x => x.Results.Count());
                this.Refresh();
            }

            int i = 0;
            int maxY = 4;
            Chart newChart = new Chart();
            newChart.Size = new Size(500, 500);
            newChart.Location = new Point(12, 124);
            ChartArea dpsArea = new ChartArea();
            dpsArea.AxisY.IntervalType = DateTimeIntervalType.Number;

            dpsArea.AxisY.Minimum = 0;
            dpsArea.AxisY.Maximum = MyTaskList.Tasks[0].Data.maxDPAV;
            dpsArea.AxisX.Minimum = 0;
            dpsArea.AxisX.Maximum = maxY;
            string dpsPartyCharting = "Damage Deal";
            newChart.Series.Add(dpsPartyCharting);
            newChart.Series[dpsPartyCharting].ChartType = SeriesChartType.StackedBar;


            newChart.Series[dpsPartyCharting].Points.AddXY(maxY - i, MyTaskList.Tasks[0].Data.avgDPAV);
            newChart.Series[dpsPartyCharting].Points[i].Label = $"Party DPAV: {MyTaskList.Tasks[0].Data.avgDPAV:f} ";
            foreach (PartyUnit unit in MyTaskList.Tasks[0].Data.PartyUnits)
            {
                i++;
                newChart.Series[dpsPartyCharting].Points.AddXY(maxY - i, unit.avgDPAV);
                newChart.Series[dpsPartyCharting].Points[i].Label = $"{unit.CombatUnit} DPAV: {unit.avgDPAV:f} ";
            }




            newChart.ChartAreas.Add(dpsArea);
            this.Controls.Add(newChart);

            BtnGo.Enabled = true;




        }

        private void lblWinRate_Click(object sender, EventArgs e)
        {

        }
    }
}
