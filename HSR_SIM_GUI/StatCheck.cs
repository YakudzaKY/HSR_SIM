using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using HSR_SIM_GUI.ChartTools;
using HSR_SIM_GUI.DamageTools;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_GUI.GuiUtils;
using static HSR_SIM_LIB.Worker;
using static HSR_SIM_GUI.DamageTools.OcrUtils;
using static HSR_SIM_GUI.DamageTools.TaskUtils;
using static HSR_SIM_GUI.DamageTools.ThreadUtils;
using static HSR_SIM_GUI.Data.StatData;


namespace HSR_SIM_GUI;

public partial class StatCheck : Form
{
    /// <summary>
    ///     force new OCR rectangles
    /// </summary>
    private bool forceNewRect;

    private Thread mainThread;


    private RTaskList myTaskList;

    public StatCheck()
    {
        InitializeComponent();
    }

    private void RefreshCbs()
    {
        cbScenario.Items.Clear();
        chkProfiles.Items.Clear();
        var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\");

        foreach (var file in files) cbScenario.Items.Add(Path.GetFileName(file));
        files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\");

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

        NmbThreadsCount.Value = Environment.ProcessorCount - 2;
        mainThread = null;
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
            { Character = character, Step = step, Stat = item, Val = SearchStatDeltaByName(item) * step });
        if (!string.IsNullOrEmpty(minusItem))
            res.Add(new RStatMod
            {
                Character = character, Step = step, Stat = minusItem, Val = -SearchStatDeltaByName(minusItem) * step
            });
        return res;
    }


    //generate subtask by profile
    private List<RTask> getStatsSubTasks(string profile)
    {
        var res = new List<RTask>();
        if (rbStatImpcat.Checked)
        {
            if (!string.IsNullOrEmpty(cbCharacter.Text))
                foreach (var item in chkStats.CheckedItems)
                    for (var i = 1; i <= nmbSteps.Value; i++)
                        res.Add(new RTask
                        {
                            Scenario = cbScenario.Text,
                            Profile = profile,
                            Iterations = (int)NmbIterations.Value,
                            StatMods = GetStatMods(cbCharacter.Text, (string)item,
                                i * (int)nmbUpgradesPerStep.Value, cbStatToReplace.Text)
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
                                    Step = 1,
                                    Val = (str == RectModeEnm.Minus ? -1 : 1) * ExctractDoubleVal(statVal)
                                });
                        }
                    }


                if (statModList.Count > 0)
                {
                    statModList.Insert(0, new RStatMod { Stat = "NEW GEAR", Step = 1 });
                    res.Add(new RTask
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
        mainThread = new Thread(ThreadWork.DoWork);
        myTaskList = new RTaskList();
        myTaskList.Tasks = new List<RTask>();
        myTaskList.ThreadCount = (int)NmbThreadsCount.Value;
        foreach (var item in chkProfiles.CheckedItems)
            myTaskList.Tasks.Add(new RTask
            {
                Scenario = cbScenario.Text,
                Profile = (string)item,
                Iterations = (int)NmbIterations.Value,
                Subtasks = getStatsSubTasks((string)item)
            });


        PB1.Value = 0;
        PB1.Maximum = myTaskList.Tasks.Sum(x => x.Iterations + x.Subtasks.Sum(y => y.Iterations));
        mainThread.Start(myTaskList);

        while (mainThread.IsAlive)
        {
            Thread.Sleep(100);
            PB1.Value = myTaskList.Tasks.Sum(x => x.Results.Count + x.Subtasks.Sum(y => y.Results.Count));
            Refresh();
        }

        //clear old
        var ctr = pnlCharts.Controls.Find("Chart", false).FirstOrDefault();
        while (ctr != null)
        {
            pnlCharts.Controls.Remove(ctr);
            ctr.Dispose();
            ctr = pnlCharts.Controls.Find("Chart", false).FirstOrDefault();
        }

        foreach (var task in myTaskList.Tasks)
        {
            var newChart = ChartUtils.getChart(task);
            newChart.Name = "Chart";
            pnlCharts.Controls.Add(newChart);
        }

        BtnGo.Enabled = true;
    }

    //load distinct characters from profiles
    public void reloadProfileCharacters()
    {
        cbCharacter.Items.Clear();
        cbCharacterGrp.Items.Clear();
        foreach (var item in chkProfiles.CheckedItems)
        {
            var units =
                XMLLoader.ExctractPartyFromXml(AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\" + item);
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
        foreach (Control ctrl in gbMinus.Controls) IniF.IniWriteValue("StatCheckForm", ctrl.Name, ctrl.Text);
        foreach (Control ctrl in gbPlus.Controls) IniF.IniWriteValue("StatCheckForm", ctrl.Name, ctrl.Text);
    }

    private void StatCheck_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveGearReplaceValues();
    }

    private void btnImport_Click(object sender, EventArgs e)
    {
        var keyVal = new OcrUtils().GetComparisonItemStat(this, forceNewRect);
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
}