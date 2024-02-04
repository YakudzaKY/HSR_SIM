using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using HSR_SIM_GUI.DamageTools;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using Newtonsoft.Json.Linq;

namespace HSR_SIM_GUI.ChartTools;

internal static class ChartUtils
{
    /// <summary>
    ///     generate Chart control by results
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Chart getChart(TaskUtils.RTask task)
    {
        var i = 0;
        var newChart = new Chart();
        newChart.Size = new Size(380, 380);
        newChart.Palette = ChartColorPalette.Chocolate;
        newChart.Titles.Add(new Title($"{task.Profile} wr:{task.WinRate:f}%"));
        newChart.Titles.Add(new Title($"Win stats: cycles:{task.Data.Cycles:f}     totalAV:{task.Data.TotalAV:f}"));
        if (task.WinRate < 100)
            newChart.Titles.Add(new Title($"Defeat stats: cycles:{task.Data.DefeatCycles:f}"));
        //CharArea
        var dpsArea = new ChartArea("primaryArea");

        //Axis
        dpsArea.AxisY.IntervalType = DateTimeIntervalType.Number;
        dpsArea.AxisY.Minimum = 0;
        dpsArea.AxisY.Maximum = task.Data.maxDPAV;
        dpsArea.AxisY.Interval = 100;
        dpsArea.AxisX.CustomLabels.Add(new CustomLabel(i, i + 0.001, "Party", 0, LabelMarkStyle.None));
        dpsArea.AxisX.IsLabelAutoFit = false;
        dpsArea.AxisX.IsReversed = true;


        //Legend
        var dpsPartyCharting = "Total DPAV";
        var directDpav = nameof(DirectDamage);
        var dotDpav = nameof(DoTDamage);
        var toughnessBreak = nameof(ToughnessBreak);
        var toughnessBreakDoTDamage = nameof(ToughnessBreakDoTDamage);

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
        foreach (var unit in task.Data.PartyUnits.OrderByDescending(x => x.Value.avgDPAV))
        {
            i++;
            var unitLabel = new CustomLabel(i, i + 0.001, unit.Key, 0, LabelMarkStyle.None);
            dpsArea.AxisX.CustomLabels.Add(unitLabel);
            newChart.Series[dpsPartyCharting].Points.AddXY(i, unit.Value.avgDPAV);
            newChart.Series[dpsPartyCharting].Points[i].Label = $"{unit.Value.avgDPAV:f}";

            foreach (var kindDmg in unit.Value.avgByTypeDPAV.Where(x => x.Value > 0))
            {
                var ndx = newChart.Series[kindDmg.Key.Name].Points.AddXY(i, kindDmg.Value);
                newChart.Series[kindDmg.Key.Name].Points[ndx].Label = $"{kindDmg.Value:f}";
            }
        }

        newChart.ChartAreas.Add(dpsArea);
        var partyDpsLegend = new Legend("partyDps");
        partyDpsLegend.DockedToChartArea = dpsArea.Name;
        partyDpsLegend.IsDockedInsideChartArea = false;
        newChart.Legends.Add(partyDpsLegend);
        newChart.Dock = DockStyle.Top;

        if (task.Subtasks.Count > 0)
        {
            //extend size 
            newChart.Size = new Size(newChart.Size.Width, newChart.Size.Height * 2);
            var statsChartArea = new ChartArea();
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
                newChart.Series[statMod.Stat].Points.Last().Label = $"wr:{subtask.WinRate:f}%";
                //newChart.Series[statMod.Stat].Points.Last().Label = $"wr:{subtask.Data.WinRate:f}% wcl:{subtask.Data.Cycles:f} dcl:{subtask.Data.DefeatCycles:f}";// extended stats
            }
        }

        GuiUtils.ApplyDarkLightTheme(newChart);
        return newChart;
    }
}