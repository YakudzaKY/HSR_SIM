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
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using HSR_SIM_LIB.Utils;
using ImageMagick;
using HSR_SIM_LIB.TurnBasedClasses;
using Tesseract;
using static HSR_SIM_LIB.Worker;
namespace HSR_SIM_GUI
{

    public partial class StatCheck : Form
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;

        public static Bitmap CaptureScreen(bool CaptureMouse)
        {
            Bitmap result = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);

            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

                    if (CaptureMouse)
                    {
                        CURSORINFO pci;
                        pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

                        if (GetCursorInfo(out pci))
                        {
                            if (pci.flags == CURSOR_SHOWING)
                            {
                                DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                                g.ReleaseHdc();
                            }
                        }
                    }
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }

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

        private Dictionary<string, string> subStatsUpgrades = new Dictionary<string, string>()
        {
            { "spd_fix", "2,3" }
            ,{ "hp_fix", "38,103755" }
            ,{ "atk_fix", "19,051877" }
            ,{ "def", "19,051877" }
            ,{ "hp_prc", "0,03888" }
            ,{ "atk_prc", "0,03888" }
            ,{ "def_prc", "0,0486" }
            ,{ "break_dmg_prc", "0,05832" }
            ,{ "effect_hit_prc", "0,03888" }
            ,{ "effect_res_prc", "0,03888" }
            ,{ "crit_rate_prc", "0,02916" }
            ,{ "crit_dmg_prc", "0,05832" }

        };

        private Dictionary<string, string> mainStatsUpgrades = new Dictionary<string, string>()
        {
            { "spd_fix", "1,4" }
            ,{ "hp_fix", "39,5136" }
            ,{ "atk_fix", "19,7568" }
            ,{ "hp_prc", "0,024192" }
            ,{ "atk_prc", "0,024192" }
            ,{ "def_prc", "0,03024" }
            ,{ "break_dmg_prc", "0,036277" }
            ,{ "effect_hit_prc", "0,024192" }
            ,{ "sp_rate_prc", "0,010886" }
            ,{ "heal_rate_prc", "0,019354" }
            ,{ "crit_rate_prc", "0,018144" }
            ,{ "crit_dmg_prc", "0,036288" }
            //elements
            ,{ "wind_dmg_prc", "0,021773" }
            ,{ "physical_dmg_prc", "0,021773" }
            ,{ "fire_dmg_prc", "0,021773" }
            ,{ "ice_dmg_prc", "0,021773" }
            ,{ "lightning_dmg_prc", "0,021773" }
            ,{ "quantum_dmg_prc", "0,021773" }
            ,{ "imaginary_dmg_prc", "0,021773" }
        };

        private bool isDown;
        private int initialX;
        private int initialY;
        private int finishX;
        private int finishY;
        private Rectangle selectRect;

        System.Drawing.Graphics formGraphics;

        private void LoadStatTable(Dictionary<string, string> table)
        {
            cbStatToReplace.Items.Clear();
            dgStatUpgrades.Rows.Clear();
            chkStats.Items.Clear();

            foreach (KeyValuePair<string, string> item in table)
            {
                dgStatUpgrades.Rows.Add(new DataGridViewRow() { Cells = { new DataGridViewTextBoxCell() { Value = item.Key }, new DataGridViewTextBoxCell() { Value = item.Value } } });
                chkStats.Items.Add(item.Key.ToString());
                cbStatToReplace.Items.Add(item.Key);
            }
        }
        private void StatCheck_Load(object sender, EventArgs e)
        {
            LoadStatTable(subStatsUpgrades);


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



            //replace calc items
            var mainQuery = (from p in subStatsUpgrades.Keys select p)
                .Union(from p in mainStatsUpgrades.Keys select p)
                .Distinct();
            List<string> stats = mainQuery.ToList();
            int startX = 10;
            int startY = 15;
            for (int i = 0; i < 5; i++)
            {
                foreach (string str in new ArrayList() { "Minus", "Plus" })
                {
                    ComboBox newCb = new ComboBox()
                    { Name = $"cb{str}Stat{i:d}", Location = new Point(startX, startY + 20 * i), Width = 100 };
                    TextBox tbBox = new TextBox()
                    {
                        Name = $"txt{str}Stat{i:d}",
                        Location = new Point(startX + newCb.Width + 10, startY + 20 * i),
                        Width = 50
                    };
                    foreach (string statItem in stats)
                    {
                        newCb.Items.Add(statItem);
                    }
                    newCb.Text = IniF.IniReadValue("StatCheckForm", newCb.Name);
                    newCb.TabIndex = i;
                    tbBox.Text = IniF.IniReadValue("StatCheckForm", tbBox.Name);
                    tbBox.TabIndex = i;
                    gbGearReplace.Controls.Find($"gb{str}", false).First().Controls.Add(newCb);
                    gbGearReplace.Controls.Find($"gb{str}", false).First().Controls.Add(tbBox);

                }



            }

            int.TryParse(IniF.IniReadValue("StatCheckForm", "initialX"), out initialX);
            int.TryParse(IniF.IniReadValue("StatCheckForm", "initialY"), out initialY);
            int.TryParse(IniF.IniReadValue("StatCheckForm", "finishX"), out finishX);
            int.TryParse(IniF.IniReadValue("StatCheckForm", "finishY"), out finishY);

            Utils.ApplyDarkLightTheme(this);
        }


        private double SearchStatDeltaByName(string item)
        {

            for (int i = 0; i < dgStatUpgrades.RowCount; i++)
            {
                if (dgStatUpgrades.Rows[i].Cells[0].Value.Equals(item))
                {
                    return Double.Parse(dgStatUpgrades.Rows[i].Cells[1].Value.ToString());
                }
            }
            return 0;
        }

        private double ExctractDoubleVal(string inputStr)
        {
            inputStr = inputStr.Replace(".", ",");
            return inputStr.EndsWith("%") ?
                double.Parse(inputStr.Substring(0, inputStr.Length - 1)) / 100 : double.Parse(inputStr);

        }
        //get Stat mods by checked item(one mod atm)
        private List<Worker.RStatMod> GetStatMods(string character, string item, int step, string minusItem = null)
        {

            List<Worker.RStatMod> res = new List<Worker.RStatMod>();
            res.Add(new Worker.RStatMod() { Character = character, Step = step, Stat = item, Val = SearchStatDeltaByName(item) * step });
            if (!String.IsNullOrEmpty(minusItem))
                res.Add(new Worker.RStatMod() { Character = character, Step = step, Stat = minusItem, Val = -SearchStatDeltaByName(minusItem) * step });
            return res;
        }


        //generate subtask by profile
        private List<RTask> getStatsSubTasks(string profile)
        {
            List<RTask> res = new List<RTask>();
            if (rbStatImpcat.Checked)
            {
                if (!String.IsNullOrEmpty(cbCharacter.Text))
                    foreach (var item in chkStats.CheckedItems)
                    {
                        for (int i = 1; i <= nmbSteps.Value; i++)
                            res.Add(new RTask()
                            {
                                Scenario = cbScenario.Text,
                                Profile = profile,
                                Iterations = (int)NmbIterations.Value,
                                StatMods = GetStatMods(cbCharacter.Text, (string)item,
                                    i * (int)nmbUpgradesPerStep.Value, cbStatToReplace.Text)
                            });
                    }
            }
            else if (rbGearReplace.Checked)
            {
                if (!String.IsNullOrEmpty(cbCharacterGrp.Text))
                {
                    List<RStatMod> statModList = new List<RStatMod>();
                    for (int i = 0; i < 5; i++)
                    {
                        string statName = gbMinus.Controls.Find($"cbMinusStat{i}", false).First().Text;
                        string statVal = gbMinus.Controls.Find($"txtMinusStat{i}", false).First().Text;
                        if (!string.IsNullOrEmpty(statName) && !string.IsNullOrEmpty(statVal))
                            statModList.Add(new RStatMod() { Character = cbCharacterGrp.Text, Stat = statName, Step = 1, Val = -ExctractDoubleVal(statVal) });
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        string statName = gbPlus.Controls.Find($"cbPlusStat{i}", false).First().Text;
                        string statVal = gbPlus.Controls.Find($"txtPlusStat{i}", false).First().Text;
                        if (!string.IsNullOrEmpty(statName) && !string.IsNullOrEmpty(statVal))
                            statModList.Add(new RStatMod() { Character = cbCharacterGrp.Text, Stat = statName, Step = 1, Val = ExctractDoubleVal(statVal) });
                    }

                    if (statModList.Count > 0)
                    {
                        statModList.Insert(0, new RStatMod() { Stat = "NEW GEAR", Step = 1 });
                        res.Add(new RTask()
                        {
                            Scenario = cbScenario.Text,
                            Profile = profile,
                            Iterations = (int)NmbIterations.Value,
                            StatMods = statModList
                        });
                    }
                }
            }
            return res;
        }
        private void BtnGo_Click(object sender, EventArgs e)
        {
            SaveGearReplaceValues();
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
                                     // from c in p.StatMods                               
                                 select p.StatMods.First().Stat)
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

                    var statMod = subtask.StatMods.First();
                    newChart.Series[statMod.Stat].Points.AddXY(statMod.Step, subtask.Data.avgDPAV - task.Data.avgDPAV);
                    newChart.Series[statMod.Stat].Points.Last().Label = $"wr:{subtask.Data.WinRate:f}%";



                }


            }
            ApplyDarkLightTheme(newChart);
            return newChart;
        }


        //load distinct characters from profiles
        public void reloadProfileCharacters()
        {
            cbCharacter.Items.Clear();
            cbCharacterGrp.Items.Clear();
            foreach (var item in chkProfiles.CheckedItems)
            {

                List<Unit> units =
                    XMLLoader.ExctractPartyFromXml(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\" + item);
                cbCharacter.Items.AddRange(units.Where(x => !cbCharacter.Items.Contains(x.Name)).Select(x => x.Name).ToArray());

            }

            for (int i = 0; i < cbCharacter.Items.Count; i++)
            {
                cbCharacterGrp.Items.Add(cbCharacter.Items[i]);
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

        private void btnLoadSubstats_Click(object sender, EventArgs e)
        {
            nmbSteps.Value = 4;
            nmbUpgradesPerStep.Value = 4;
            LoadStatTable(subStatsUpgrades);
        }

        private void btnMainStats_Click(object sender, EventArgs e)
        {
            nmbSteps.Value = 1;
            nmbUpgradesPerStep.Value = 15;
            LoadStatTable(mainStatsUpgrades);
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void setCalcVisible()
        {
            gbStatImpcat.Visible = rbStatImpcat.Checked;
            gbGearReplace.Visible = !gbStatImpcat.Visible;
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            setCalcVisible();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            setCalcVisible();
        }

        private void SaveGearReplaceValues()
        {
            foreach (Control ctrl in gbMinus.Controls)
            {
                IniF.IniWriteValue("StatCheckForm", ctrl.Name, ctrl.Text);
            }
            foreach (Control ctrl in gbPlus.Controls)
            {
                IniF.IniWriteValue("StatCheckForm", ctrl.Name, ctrl.Text);
            }
        }
        private void StatCheck_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveGearReplaceValues();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            Dictionary<int, RStatWordRec> keyVal = GetValuesFromScreen();
            foreach (Control ctrl in gbPlus.Controls)
            {
                ctrl.Text = string.Empty;
            }
            foreach (Control ctrl in gbMinus.Controls)
            {
                ctrl.Text = string.Empty;
            }
            for (int i = 0; i < keyVal.Count; i++)
            {
                gbMinus.Controls.Find($"cbMinusStat{i}", false).First().Text = keyVal[i].Key;
                gbMinus.Controls.Find($"txtMinusStat{i}", false).First().Text = keyVal[i].Value;
            }


            for (int i = 0; i < keyVal.Count; i++)
            {
                gbPlus.Controls.Find($"cbPlusStat{i}", false).First().Text = keyVal[i].Key;
                gbPlus.Controls.Find($"txtPlusStat{i}", false).First().Text = keyVal[i].Value;
            }
        }

        private record RStatWordRec
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public int listNum { get; set; }

        }
        private Dictionary<int, RStatWordRec> GetValuesFromScreen()
        {
            Dictionary<int, RStatWordRec> res = new Dictionary<int, RStatWordRec>();
            SetForegroundWindow(FindWindow(null, "Honkai: Star Rail"));

            Bitmap hsrScreen = CaptureScreen(false);
            using (var form = new Form())
            {
                if (finishX > 0 && finishY > 0 && finishX > initialX && finishY > initialY)
                {
                    selectRect = new Rectangle(initialX, initialY, finishX - initialX, finishY - initialY);
                }
                else
                {
                    form.Size = hsrScreen.Size;
                    PictureBox pictureBox1 = new PictureBox();
                    using Graphics gfx = Graphics.FromImage(hsrScreen);
                    gfx.DrawString("SELECT TEXT PARSE AREA(HOLD DOWN MOUSE AND MOVE)",
                        new("Tahoma", 13, FontStyle.Bold), new SolidBrush(Color.Red),
                        new PointF(hsrScreen.Width / 3, 150));
                    pictureBox1.Size = hsrScreen.Size;
                    pictureBox1.Image = hsrScreen;
                    form.Controls.Add(pictureBox1);
                    form.FormBorderStyle = FormBorderStyle.None;
                    pictureBox1.MouseMove += PictureBox_MouseMove;
                    pictureBox1.MouseDown += PictureBox_MouseDown;
                    pictureBox1.MouseUp += PictureBox_MouseUp;
                    SetForegroundWindow(form.Handle);
                    form.ShowDialog();
                }

                string loc = "rus";
                hsrScreen = hsrScreen.Clone(selectRect, hsrScreen.PixelFormat);
                SetForegroundWindow(this.Handle);

                //todo clear stats values and names
                using (var engine = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory + "tessdata", loc, EngineMode.TesseractAndLstm))
                {
                    using (var img = PixConverter.ToPix(hsrScreen))
                    {
                        using (var page = engine.Process(img))
                        {
                            // hsrScreen.Save("test.jpeg");
                            
                            var text = page.GetText();
                            text = text.Replace("\n\n", "\n");
                            if (text.EndsWith("\n"))
                                text = text.Substring(0, text.Length - 1);

                            List<string> strings = text.Split('\n').ToList();

                            //replace some shit into stat words
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\tessdata\\{loc}.xml");
                            XmlElement xRoot = xDoc.DocumentElement;
                            int toList = 0;
                            int stringNumberIndex = 0;
                            int stringNumber = 0;
                            if (xRoot != null)
                            {
                                //parse all items
                                foreach (XmlElement xnode in xRoot)
                                {
                                    if (xnode.Name == "replacer")
                                    {
                                        string wordFrom = xnode.Attributes.GetNamedItem("from").Value.Trim();
                                        string wordTo = xnode.Attributes.GetNamedItem("to")?.Value.Trim();
                                        for (int i = 0; i < strings.Count; i++)
                                        {
                                            if (strings[i].IndexOf(wordFrom) > 0)
                                                strings[i] = wordTo;
                                            //we detect number string. So first item stat ends
                                            if (stringNumberIndex == 0 && int.TryParse(strings[i].Substring(0, 1), out stringNumber))
                                                stringNumberIndex = i;

                                        }
                                    }
                                }
                            }

                            for (int i = 0; i < stringNumberIndex; i++)
                            {

                                string val = strings[i + stringNumberIndex];
                                string key = strings[i] + (val.EndsWith("%") ? "_prc" : "_fix");
                                res.Add(res.Count, new RStatWordRec() { listNum = 0, Key = key, Value = val });
                            }

                            for (int i = (res.Count * 2); i < (strings.Count / 2 - res.Count); i++)
                            {

                                string val = strings[i + stringNumberIndex];
                                string key = strings[i] + (val.EndsWith("%") ? "_prc" : "_fix");
                                res.Add(res.Count, new RStatWordRec() { listNum = 0, Key = key, Value = val });
                            }


                        }
                    }
                }

            }

            return res;
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isDown = false;
            finishX = e.X;
            finishY = e.Y;
            IniF.IniWriteValue("StatCheckForm", "finishX", finishX.ToString());
            IniF.IniWriteValue("StatCheckForm", "finishY", finishY.ToString());
            if (selectRect.Height > 0 && selectRect.Width > 0)
                ((Form)((Control)sender).Parent)?.Close();
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            isDown = true;
            initialX = e.X;
            initialY = e.Y;
            IniF.IniWriteValue("StatCheckForm", "initialX", initialX.ToString());
            IniF.IniWriteValue("StatCheckForm", "initialY", initialY.ToString());
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDown == true)
            {
                ((Control)sender).Refresh();
                Pen drwaPen = new Pen(Color.Red, 3);
                int width = e.X - initialX, height = e.Y - initialY;
                selectRect = new Rectangle(Math.Min(e.X, initialX),
                   Math.Min(e.Y, initialY),
                   Math.Abs(e.X - initialX),
                   Math.Abs(e.Y - initialY));
                formGraphics = ((Control)sender).CreateGraphics();
                formGraphics.DrawRectangle(drwaPen, selectRect);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            initialX = 0;
            initialY = 0;
            finishX = 0;
            finishY = 0;
        }
    }
}
