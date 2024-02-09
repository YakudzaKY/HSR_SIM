using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HSR_SIM_GUI.ChartTools;
using HSR_SIM_GUI.DamageTools;
using HSR_SIM_GUI.TaskTools;
using HSR_SIM_GUI.ThreadTools;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_GUI.GuiUtils;
using static HSR_SIM_LIB.Worker;
using static HSR_SIM_GUI.DamageTools.OcrUtils;
using static HSR_SIM_GUI.Data.StatData;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace HSR_SIM_GUI;

public partial class StatCheck : Form
{
    /// <summary>
    ///     force new OCR rectangles
    /// </summary>
    private bool forceNewRect;

    private AggregateThread aggThread;

    private bool interruptFlag;
    private List<SimTask> myTaskList;

    public StatCheck()
    {
        InitializeComponent();
    }

    private void RefreshCbs()
    {
        cbScenario.Items.Clear();
        chkProfiles.Items.Clear();
        var files = Directory.GetFiles(GetScenarioPath(), "*.xml");

        foreach (var file in files) cbScenario.Items.Add(Path.GetFileName(file));
        files = Directory.GetFiles(GetProfilePath(), "*.xml");

        foreach (var file in files) chkProfiles.Items.Add(Path.GetFileName(file));
    }

    private void LoadStatTable(Dictionary<string, string> table)
    {
        cbStatToReplace.Items.Clear();
        dgStatUpgrades.Rows.Clear();
        chkStats.Items.Clear();

        foreach (var item in table)
        {
            dgStatUpgrades.Rows.Add(new DataGridViewRow
            {
                Cells =
                {
                    new DataGridViewTextBoxCell { Value = item.Key }, new DataGridViewTextBoxCell { Value = item.Value }
                }
            });
            chkStats.Items.Add(item.Key);
            cbStatToReplace.Items.Add(item.Key);
        }
    }

    private void StatCheck_Load(object sender, EventArgs e)
    {
        RefreshCbs();
        cbScenario.Text = IniF.IniReadValue("form", "Scenario");

        for (var i = 0; i < chkProfiles.Items.Count; i++)
            if ((string)chkProfiles.Items[i] == IniF.IniReadValue("form", "Profile"))
            {
                chkProfiles.SetItemChecked(i, true);
                break;
            }

        NmbThreadsCount.Value = Math.Max((Environment.ProcessorCount/2) - 1,1);
        aggThread = null;
        reloadProfileCharacters();

        //replace calc items
        var mainQuery = (from p in subStatsUpgrades.Keys select p)
            .Union(from p in mainStatsUpgrades.Keys select p)
            .Distinct();
        var stats = mainQuery.ToList();
        var startX = 10;
        var startY = 15;
        for (var i = 0; i < 5; i++)
            foreach (var str in (RectModeEnm[])Enum.GetValues(typeof(RectModeEnm)))

            {
                var newCb = new ComboBox
                { Name = $"cb{str}Stat{i:d}", Location = new Point(startX, startY + 22 * i), Width = 100 };
                Control tbBox;
                if (i == 0)
                    tbBox = new Label
                    {
                        Name = $"lbl{str}Stat{i:d}",
                        Location = new Point(startX + newCb.Width + 10, startY + 22 * i),
                        Width = 50,
                        Text = "MAX."
                    };
                else
                    tbBox = new TextBox
                    {
                        Name = $"txt{str}Stat{i:d}",
                        Location = new Point(startX + newCb.Width + 10, startY + 22 * i),
                        Width = 50
                    };

                foreach (var statItem in stats) newCb.Items.Add(statItem);
                newCb.Text = IniF.IniReadValue("StatCheckForm", newCb.Name);
                newCb.TabIndex = i;
                if (i > 0) tbBox.Text = IniF.IniReadValue("StatCheckForm", tbBox.Name);

                tbBox.TabIndex = i;
                gbGearReplace.Controls.Find($"gb{str}", false).First().Controls.Add(newCb);
                gbGearReplace.Controls.Find($"gb{str}", false).First().Controls.Add(tbBox);
            }

        ApplyDarkLightTheme(this);
    }


    private double SearchStatDeltaByName(string item)
    {
        for (var i = 0; i < dgStatUpgrades.RowCount; i++)
            if (dgStatUpgrades.Rows[i].Cells[0].Value.Equals(item))
                return double.Parse(dgStatUpgrades.Rows[i].Cells[1].Value.ToString());
        return 0;
    }

    private static double SearchStatDeltaByNameInMainStats(string item)
    {
        return ExctractDoubleVal(mainStatsUpgrades.First(x => x.Key.Equals(item)).Value);
    }

    private static double SearchStatBaseByNameInMainStats(string item)
    {
        return ExctractDoubleVal(mainStatsBase.First(x => x.Key.Equals(item)).Value);
    }

    private static double ExctractDoubleVal(string inputStr)
    {
        inputStr = inputStr.Replace('.', ',');
        return inputStr.EndsWith('%')
            ? double.Parse(inputStr.Substring(0, inputStr.Length - 1)) / 100
            : double.Parse(inputStr);
    }

    //get Stat mods by checked item(one mod atm)
    private List<RStatMod> GetStatMods(string character, string item, int step, string minusItem = null)
    {
        var res = new List<RStatMod>();
        res.Add(new RStatMod
        { Character = character, Stat = item, Val = SearchStatDeltaByName(item) * step });
        if (!string.IsNullOrEmpty(minusItem))
            res.Add(new RStatMod
            {
                Character = character,
                Stat = minusItem,
                Val = -SearchStatDeltaByName(minusItem) * step
            });
        return res;
    }


    //generate subtask by profile
    private List<SimTask> GetStatsSubTasks(string profile, SimTask simTask)
    {
        var res = new List<SimTask>();
        if (rbStatImpcat.Checked)
        {
            if (!string.IsNullOrEmpty(cbCharacter.Text))
                foreach (var item in chkStats.CheckedItems)
                    for (var i = 1; i <= nmbSteps.Value; i++)
                        res.Add(new SimTask
                        {
                            Scenario = GetScenarioPath() + cbScenario.Text,
                            Profile =  GetProfilePath()+ profile,
                            Parent = simTask,
                            Step=i * (int)nmbUpgradesPerStep.Value,
                            StatMods = GetStatMods(cbCharacter.Text, (string)item,
                                i * (int)nmbUpgradesPerStep.Value,  cbStatToReplace.Text)
                        });
        }
        else if (rbGearReplace.Checked)
        {
            if (!string.IsNullOrEmpty(cbCharacterGrp.Text))
            {
                var statModList = new List<RStatMod>();
                foreach (var str in (RectModeEnm[])Enum.GetValues(typeof(RectModeEnm)))
                    for (var i = 0; i < 5; i++)
                    {
                        var statName = Controls.Find($"cb{str}Stat{i}", true).First().Text;
                        if (!string.IsNullOrEmpty(statName))
                        {
                            var statVal = i > 0
                                ? Controls.Find($"txt{str}Stat{i}", true).First().Text
                                : (SearchStatBaseByNameInMainStats(statName) +
                                   15 * SearchStatDeltaByNameInMainStats(statName)).ToString();
                            if (!string.IsNullOrEmpty(statName) && !string.IsNullOrEmpty(statVal))
                                statModList.Add(new RStatMod
                                {
                                    Character = cbCharacterGrp.Text,
                                    Stat = statName,
                                    Val = (str == RectModeEnm.Minus ? -1 : 1) * ExctractDoubleVal(statVal)
                                });
                        }
                    }


                if (statModList.Count > 0)
                {
                    statModList.Insert(0, new RStatMod { Stat = "NEW GEAR"});
                    res.Add(new SimTask
                    {
                        Scenario = GetScenarioPath() + cbScenario.Text,
                        Profile = GetProfilePath() + profile,
                        StatMods = statModList,
                        Step=1,
                        Parent = simTask
                    });
                }
            }
        }

        return res;
    }

    private void DoJob()
    {
        ThreadJob threadJob = new ThreadJob(myTaskList, (int)NmbIterations.Value);


        interruptFlag = false;
        //save ini file(because app can crash may be if CPU overheated depend)
        SaveGearReplaceValues();

        //check four double call this proc
        if (this.aggThread?.IsAlive ?? false)
            return;

        BtnGo.Invoke((MethodInvoker)delegate
        {
            BtnGo.Enabled = false;
        });

        btnCancel.Invoke((MethodInvoker)delegate
        {
            btnCancel.Visible = true;
        });

        this.aggThread = new AggregateThread(threadJob,(int)NmbThreadsCount.Value);
      


        PB1.Invoke((MethodInvoker)delegate
        {
            PB1.Value = 0;
        });
        int valMax = myTaskList.Count * threadJob.Iterations;
        PB1.Invoke((MethodInvoker)delegate
        {
            PB1.Maximum = valMax;
        });

        this.aggThread.Start();


        do
        {

            var stDate = DateTime.Now;
            if (interruptFlag)
                this.aggThread.Interrupt();

            int progressBefore = aggThread.Progress();

            Thread.Sleep(1000);

            int progress = aggThread.Progress();
            PB1.Invoke((MethodInvoker)delegate
            {
                PB1.Value = progress;
            });
            var crDate = DateTime.Now;
            var diffInSeconds = (crDate - stDate).TotalSeconds;
            double performance = diffInSeconds > 0 ? ((progress - progressBefore) / diffInSeconds) : 0; //sims per sec
            int eta = performance>0? (int)((valMax - progress) / performance):0; //sec estimate 
            int etaM = (int)Math.Floor( (double)eta /60);


            string etaFormated = $"{etaM}m {eta-etaM*60}s";
            lblProgressCnt.Invoke((MethodInvoker)delegate
            {
                lblProgressCnt.Text = $"{PB1.Value}\\{PB1.Maximum}  {performance:f}\\sec    ETA:{etaFormated}";
            });
            //Refresh();
        } while (this.aggThread.IsAlive);
      
        //clear old
        var ctr = pnlCharts.Controls.Find("Chart", false).FirstOrDefault();
        while (ctr != null)
        {
            pnlCharts.Invoke((MethodInvoker)delegate
            {
                pnlCharts.Controls.Remove(ctr);
            });

            ctr.Dispose();
            ctr = pnlCharts.Controls.Find("Chart", false).FirstOrDefault();
        }
        
        foreach (var task in threadJob.CombatData.Where(x=>x.Key.Parent is null))
        {
            var newChart = ChartUtils.getChart(task,threadJob.CombatData.Where(x=>x.Key.Parent ==task.Key));
            newChart.Name = "Chart";
            pnlCharts.Invoke((MethodInvoker)delegate
            {
                pnlCharts.Controls.Add(newChart);
            });

        }

        btnCancel.Invoke((MethodInvoker)delegate
        {
            btnCancel.Visible = false;
        });

        BtnGo.Invoke((MethodInvoker)delegate
        {
            BtnGo.Enabled = true;
        });

       /* BtnGo.Invoke((MethodInvoker)delegate
        {
            BtnGo_Click(null, null);
        });*/
       
    }

    private async Task DoSomeJob()
    {
        await Task.Run(DoJob);
    }
    private void BtnGo_Click(object sender, EventArgs e)
    {
        //generate task list
        myTaskList = new List<SimTask>();

        foreach (var item in chkProfiles.CheckedItems)
        {
            // first parent task
            SimTask prnt = new SimTask()
            {
                Scenario = GetScenarioPath() + cbScenario.Text,
                Profile = GetProfilePath() + (string)item
            };
            myTaskList.Add(prnt);
            //childs
            myTaskList.AddRange(GetStatsSubTasks((string)item,prnt));

        }

        DoSomeJob();
    }

    //load distinct characters from profiles
    public void reloadProfileCharacters()
    {
        cbCharacter.Items.Clear();
        cbCharacterGrp.Items.Clear();
        foreach (var item in chkProfiles.CheckedItems)
        {
            var units =
                XMLLoader.ExctractPartyFromXml(GetProfilePath() + item);
            cbCharacter.Items.AddRange(units.Where(x => !cbCharacter.Items.Contains(x.Name)).Select(x => x.Name)
                .ToArray());
        }

        for (var i = 0; i < cbCharacter.Items.Count; i++) cbCharacterGrp.Items.Add(cbCharacter.Items[i]);
    }

    private void chkProfiles_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        //delayed item check(coz in "ItemCheckEventArgs e" new and old values, but in list are old values)
        BeginInvoke((MethodInvoker)(
            reloadProfileCharacters));
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
            ctrl.Invoke((MethodInvoker)delegate
            {
                IniF.IniWriteValue("StatCheckForm", ctrl.Name, ctrl.Text);
            });


        }

        foreach (Control ctrl in gbPlus.Controls)
        {
            ctrl.Invoke((MethodInvoker)delegate
            {
                IniF.IniWriteValue("StatCheckForm", ctrl.Name, ctrl.Text);
            });
        }
    }

    private void StatCheck_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveGearReplaceValues();
    }

    private void btnImport_Click(object sender, EventArgs e)
    {
        var keyVal = new OcrUtils().GetComparisonItemStat(this,ref forceNewRect);
        foreach (Control ctrl in gbPlus.Controls)
            if (ctrl is not Label)
                ctrl.Text = string.Empty;
        foreach (Control ctrl in gbMinus.Controls)
            if (ctrl is not Label)
                ctrl.Text = string.Empty;

        var i = 0;
        foreach (var item in keyVal.Where(x => x.Value.StatMode == RectModeEnm.Minus))
        {
            gbMinus.Controls.Find($"cbMinusStat{i}", false).First().Text = item.Value.Key;
            if (i > 0)
                gbMinus.Controls.Find($"txtMinusStat{i}", false).First().Text = item.Value.Value;
            i++;
        }

        i = 0;
        foreach (var item in keyVal.Where(x => x.Value.StatMode == RectModeEnm.Plus))
        {
            gbPlus.Controls.Find($"cbPlusStat{i}", false).First().Text = item.Value.Key;
            if (i > 0)
                gbPlus.Controls.Find($"txtPlusStat{i}", false).First().Text = item.Value.Value;
            i++;
        }
    }


    private void button1_Click(object sender, EventArgs e)
    {
        forceNewRect = true;
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

    private void btnCancel_Click(object sender, EventArgs e)
    {
        interruptFlag = true;
    }
}