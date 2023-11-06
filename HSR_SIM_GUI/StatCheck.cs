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
using System.Collections;
using System.Drawing.Text;
using HSR_SIM_LIB.Utils;
using ImageMagick;
using HSR_SIM_LIB.TurnBasedClasses;
using static HSR_SIM_LIB.Worker;


namespace HSR_SIM_GUI
{

    public partial class StatCheck : Form
    {
        public class ThreadWork
        {

            /// <summary>
            /// child thread func
            /// </summary>
            /// <param name="links"></param>
            public static void ProcWork(Object links)
            {
                RLinksToOjbects rLinks = links as RLinksToOjbects;
                Worker wrk = new Worker();
                wrk.LoadScenarioFromXml(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\" + rLinks.Task.Scenario, AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\" + rLinks.Task.Profile);
                wrk.ApplyModes(rLinks.Task.StatMods);
                wrk.GetCombatResult(rLinks.Result);

            }


            /// <summary>
            /// main thread work func
            /// </summary>
            /// <param name="taskList"></param>
            public static void DoWork(Object taskList)
            {
                List<Thread> myThreads = new List<Thread>();
                RTaskList myTaskList = taskList as RTaskList;
                for (int i = 0; i < myTaskList.ThreadCount; i++)
                {
                    Thread thd = new Thread(new ParameterizedThreadStart(ThreadWork.ProcWork));
                    myThreads.Add(thd);

                }

                var mainQuery = (from p in myTaskList.Tasks
                                 from c in p.Subtasks
                                 select c).Union(from p in myTaskList.Tasks select p)
                    .Distinct();

                while (mainQuery.Any(x => !x.Fetched))
                {
                    //queue tasks
                    foreach (Thread thr in (List<Thread>)myThreads.Where(x => x.IsAlive == false).ToList())
                    {

                        RTask task = mainQuery.FirstOrDefault(x => x.Results != null && x.Results.Count() < x.Iterations);

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
                    foreach (RTask fetchTask in mainQuery.Where(x => !x.Fetched && x.Results.Count(y => y.Success != null) == x.Iterations))
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

                                Type[] typeArray = new Type[] { typeof(DirectDamage), typeof(DoTDamage), typeof(ToughnessBreakDoTDamage), typeof(ToughnessBreak) };

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

        private Thread mainThread;
        public record RTask
        {
            public string Scenario;
            public string Profile;
            public List<Worker.RCombatResult> Results = new List<Worker.RCombatResult>();
            public bool Fetched;
            public RAggregatedData Data = new RAggregatedData();
            public int Iterations { get; set; }
            public List<RTask> Subtasks { get; set; }
            public List<Worker.RStatMod> StatMods { get; set; }
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


        }

        private void RefreshCbs()
        {
            cbScenario.Items.Clear();
            chkProfiles.Items.Clear();
            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\");

            foreach (string file in files)
            {
                cbScenario.Items.Add(Path.GetFileName(file));
            }
            files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\");

            foreach (string file in files)
            {
                chkProfiles.Items.Add(Path.GetFileName(file));
            }
        }
        private void StatCheck_Load(object sender, EventArgs e)
        {
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "spd" }, new DataGridViewTextBoxCell() { Value = "2,3" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "hp" }, new DataGridViewTextBoxCell() { Value = "38,103755" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "atk" }, new DataGridViewTextBoxCell() { Value = "19,051877" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "def" }, new DataGridViewTextBoxCell() { Value = "19,051877" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "hp_prc" }, new DataGridViewTextBoxCell() { Value = "0,03888" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "atk_prc" }, new DataGridViewTextBoxCell() { Value = "0,03888" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "def_prc" }, new DataGridViewTextBoxCell() { Value = "0,0486" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "break_dmg_prc" }, new DataGridViewTextBoxCell() { Value = "0,05832" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "effect_hit_prc" }, new DataGridViewTextBoxCell() { Value = "0,03888" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "effect_res_prc" }, new DataGridViewTextBoxCell() { Value = "0,03888" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "crit_rate_prc" }, new DataGridViewTextBoxCell() { Value = "0,02916" } } });
            dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = "crit_dmg_prc" }, new DataGridViewTextBoxCell() { Value = "0,05832" } } });

            for (int i = 0; i < dgStatUpgrades.Rows.Count; i++)
            {
                chkStats.Items.Add(dgStatUpgrades.Rows[i].Cells[0].Value.ToString());
            }

            RefreshCbs();
            cbScenario.Text = IniF.IniReadValue("form", "Scenario");

            for (int i = 0; i < chkProfiles.Items.Count; i++)
            {
                if ((string)chkProfiles.Items[i] == IniF.IniReadValue("form", "Profile"))
                {
                    chkProfiles.SetItemChecked(i, true);
                    break;
                }
            }
            NmbThreadsCount.Value = Environment.ProcessorCount - 2;
            mainThread = null;
            reloadProfileCharacters();
            Utils.ApplyDarkLightTheme(this);
        }


        private double SearchStatDeltaByName(string item)
        {

            for (int i = 0; i < dgStatUpgrades.RowCount; i++)
            {
                if (dgStatUpgrades.Rows[i].Cells[0].Value == item)
                {
                    return Double.Parse(dgStatUpgrades.Rows[i].Cells[1].Value.ToString());
                }
            }
            return 0;
        }

        //get Stat mods by checked item(one mod atm)
        private List<Worker.RStatMod> GetStatMods(string character, string item, int step)
        {
            List<Worker.RStatMod> res = new List<Worker.RStatMod>() { new Worker.RStatMod() { Character = character, Step = step, Stat = item, Val = SearchStatDeltaByName(item) * step } };


            return res;
        }


        //generate subtask by profile
        private List<RTask> getStatsSubTasks(string profile)
        {
            List<RTask> res = new List<RTask>();
            if (!String.IsNullOrEmpty(cbCharacter.Text))
                foreach (var item in chkStats.CheckedItems)
                {
                    for (int i = 1; i <= nmbSteps.Value; i++)
                        res.Add(new RTask()
                        {
                            Scenario = cbScenario.Text,
                            Profile = profile,
                            Iterations = (int)NmbIterations.Value,
                            StatMods = GetStatMods(cbCharacter.Text, (string)item, i * (int)nmbUpgradesPerStep.Value)
                        });
                }
            return res;
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
            foreach (var item in chkProfiles.CheckedItems)
            {
                MyTaskList.Tasks.Add(new RTask()
                {
                    Scenario = cbScenario.Text,
                    Profile = (string)item,
                    Iterations = (int)NmbIterations.Value,
                    Subtasks = getStatsSubTasks((string)item),
                });
            }


            PB1.Value = 0;
            PB1.Maximum = MyTaskList.Tasks.Sum(x => x.Iterations + x.Subtasks.Sum(y => y.Iterations));
            mainThread.Start(MyTaskList);

            while (mainThread.IsAlive)
            {
                Thread.Sleep(100);
                PB1.Value = MyTaskList.Tasks.Sum(x => x.Results.Count() + x.Subtasks.Sum(y => y.Results.Count()));
                this.Refresh();
            }

            //clear old
            Control ctr = pnlCharts.Controls.Find("Chart", false).FirstOrDefault();
            while (ctr != null)
            {

                pnlCharts.Controls.Remove(ctr);
                ctr.Dispose();
                ctr = pnlCharts.Controls.Find("Chart", false).FirstOrDefault();

            }
            foreach (RTask task in MyTaskList.Tasks)
            {
                Chart newChart = getChart(task);
                newChart.Name = "Chart";
                pnlCharts.Controls.Add(newChart);
            }




            BtnGo.Enabled = true;




        }

        /// <summary>
        /// generate Chart control by results
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private Chart getChart(RTask task)
        {
            int i = 0;
            Chart newChart = new Chart();
            newChart.Size = new Size(380, 380);
            newChart.Palette = ChartColorPalette.Chocolate;
            newChart.Titles.Add(new Title(task.Profile));
            newChart.Titles.Add(new Title($"wr:{task.Data.WinRate:f}%     cycles:{task.Data.Cycles:f}     totalAV:{task.Data.TotalAV:f}"));
            //CharArea
            ChartArea dpsArea = new ChartArea("primaryArea");

            //Axis
            dpsArea.AxisY.IntervalType = DateTimeIntervalType.Number;
            dpsArea.AxisY.Minimum = 0;
            dpsArea.AxisY.Maximum = task.Data.maxDPAV;
            dpsArea.AxisY.Interval = 100;
            dpsArea.AxisX.CustomLabels.Add(new CustomLabel(i, i + 0.001, "Party", 0, LabelMarkStyle.None));
            dpsArea.AxisX.IsLabelAutoFit = false;
            dpsArea.AxisX.IsReversed = true;


            //Legend
            string dpsPartyCharting = "Total DPAV";
            string directDpav = nameof(DirectDamage);
            string dotDpav = nameof(DoTDamage);
            string toughnessBreak = nameof(ToughnessBreak);
            string toughnessBreakDoTDamage = nameof(ToughnessBreakDoTDamage);

            //Bars
            newChart.Series.Add(dpsPartyCharting);
            newChart.Series[dpsPartyCharting].ChartType = SeriesChartType.Bar;
            newChart.Series[dpsPartyCharting].Color = Color.DarkRed;

            newChart.Series.Add(directDpav);
            newChart.Series[directDpav].ChartType = SeriesChartType.Bar;
            newChart.Series[directDpav].Color = Color.Chocolate;

            newChart.Series.Add(dotDpav);
            newChart.Series[dotDpav].ChartType = SeriesChartType.Bar;
            newChart.Series[dotDpav].Color = Color.BlueViolet;

            newChart.Series.Add(toughnessBreak);
            newChart.Series[toughnessBreak].ChartType = SeriesChartType.Bar;
            newChart.Series[toughnessBreak].Color = Color.DarkGray;

            newChart.Series.Add(toughnessBreakDoTDamage);
            newChart.Series[toughnessBreakDoTDamage].ChartType = SeriesChartType.Bar;
            newChart.Series[toughnessBreakDoTDamage].Color = Color.DarkCyan;


            //Fill party bar
            newChart.Series[dpsPartyCharting].Points.AddXY(i, task.Data.avgDPAV);
            newChart.Series[dpsPartyCharting].Points[i].Label = $"{task.Data.avgDPAV:f}";


            //Unit bar
            foreach (PartyUnit unit in task.Data.PartyUnits.OrderByDescending(x => x.avgDPAV))
            {
                i++;
                CustomLabel unitLabel = new CustomLabel(i, i + 0.001, unit.CombatUnit, 0, LabelMarkStyle.None);
                dpsArea.AxisX.CustomLabels.Add(unitLabel);
                newChart.Series[dpsPartyCharting].Points.AddXY(i, unit.avgDPAV);
                newChart.Series[dpsPartyCharting].Points[i].Label = $"{unit.avgDPAV:f}";

                foreach (var kindDmg in unit.avgByTypeDPAV.Where(x => x.Value > 0))
                {

                    int ndx = newChart.Series[kindDmg.Key.Name].Points.AddXY(i, kindDmg.Value);
                    newChart.Series[kindDmg.Key.Name].Points[ndx].Label = $"{kindDmg.Value:f}";
                }

            }

            newChart.ChartAreas.Add(dpsArea);
            Legend partyDpsLegend = new Legend("partyDps");
            partyDpsLegend.DockedToChartArea = dpsArea.Name;
            partyDpsLegend.IsDockedInsideChartArea = false;
            newChart.Legends.Add(partyDpsLegend);
            newChart.Dock = DockStyle.Top;

            if (task.Subtasks.Count > 0)
            {
                //extend size 
                newChart.Size = new Size(newChart.Size.Width, newChart.Size.Height * 2);
                ChartArea statsChartArea = new ChartArea();
                statsChartArea.Name = "statComparsion";
                newChart.ChartAreas.Add(statsChartArea);
                newChart.Legends.Add(new Legend(statsChartArea.Name));
                newChart.Legends[statsChartArea.Name].DockedToChartArea = statsChartArea.Name;
                newChart.Legends[statsChartArea.Name].IsDockedInsideChartArea = false;
                // newChart.Legends[statsChartArea.Name].Position = new ElementPosition(70, 58, 10, 30);

                //create series 

                var mainQuery = (from p in task.Subtasks
                                 from c in p.StatMods
                                 select c.Stat)
                    .Distinct();

                foreach (var stat in mainQuery)
                {
                    newChart.Series.Add(stat);
                    newChart.Series[stat].ChartArea = statsChartArea.Name;
                    newChart.Series[stat].ChartType = SeriesChartType.Line;
                    newChart.Series[stat].Legend = statsChartArea.Name;
                    newChart.Series[stat].BorderWidth = 3;
                    newChart.Series[stat].Points.AddXY(0, 0);
                    statsChartArea.AxisX.Minimum = 0;
                    statsChartArea.AxisX.Title = "Upgrade steps";
                    statsChartArea.AxisY.Title = "Party DPAV increase(vs normal)";

                }

                foreach (var subtask in task.Subtasks)
                {

                    foreach (var statMod in subtask.StatMods)
                    {
                        newChart.Series[statMod.Stat].Points.AddXY(statMod.Step, subtask.Data.avgDPAV - task.Data.avgDPAV);
                        newChart.Series[statMod.Stat].Points.Last().Label = $"+{statMod.Val:f}";
                    }


                }


            }
            ApplyDarkLightTheme(newChart);
            return newChart;
        }


        //load distinct characters from profiles
        public void reloadProfileCharacters()
        {
            cbCharacter.Items.Clear();
            foreach (var item in chkProfiles.CheckedItems)
            {

                List<Unit> units =
                    XMLLoader.ExctractPartyFromXml(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\" + item);
                cbCharacter.Items.AddRange(units.Where(x => !cbCharacter.Items.Contains(x.Name)).Select(x => x.Name).ToArray());

            }
        }

        private void chkProfiles_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            //delayed item check(coz in E new and old values, but in list are old values)
            this.BeginInvoke((MethodInvoker)(
                () => reloadProfileCharacters()));
        }

        private void dgStatUpgrades_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
