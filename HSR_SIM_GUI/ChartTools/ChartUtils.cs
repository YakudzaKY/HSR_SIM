using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using HSR_SIM_GUI.TaskTools;
using HSR_SIM_GUI.ThreadTools;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_GUI.ChartTools;

internal static class ChartUtils
{
    /// <summary>
    ///     Generate Chart for one completed sim job
    /// </summary>
    /// <param name="task">KeyPair SimTask-Results</param>
    /// <param name="childTasks">array of child results</param>
    /// <returns></returns>
    public static Chart GetChart(KeyValuePair<SimTask, ThreadJob.RAggregatedData> task,
        IEnumerable<KeyValuePair<SimTask, ThreadJob.RAggregatedData>> childTasks)
    {
        var i = 0;
        var newChart = new Chart();
        newChart.Size = new Size(380, 380);
        newChart.Palette = ChartColorPalette.Chocolate;
        newChart.Titles.Add(new Title($"{task.Key.SimScenario.CurrentScenario.Name} wr:{task.Value.WinRate:f}%"));
        newChart.Titles.Add(new Title($"Win stats: cycles:{task.Value.Cycles:f}     totalAV:{task.Value.TotalAV:f}"));
        if (task.Value.WinRate < 100)
            newChart.Titles.Add(new Title($"Defeat stats: cycles:{task.Value.DefeatCycles:f}"));
        //CharArea
        var dpsArea = new ChartArea("primaryArea");

        //Axis
        dpsArea.AxisY.IntervalType = DateTimeIntervalType.Number;
        dpsArea.AxisY.Minimum = 0;
        dpsArea.AxisY.Maximum = task.Value.maxDPAV;
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
        newChart.Series[dpsPartyCharting].Points.AddXY(i, task.Value.avgDPAV);
        newChart.Series[dpsPartyCharting].Points[i].Label = $"{task.Value.avgDPAV:f}";


        //Unit bar
        foreach (var unit in task.Value.PartyUnits.OrderByDescending(x => x.Value.avgDPAV))
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

        //If we have child task-> do additional chart compare with parent results
        var childTasksArr = childTasks as KeyValuePair<SimTask, ThreadJob.RAggregatedData>[] ?? childTasks.ToArray();
        if (childTasksArr.Any())
        {
            //extend size 
            newChart.Size = new Size(newChart.Size.Width, newChart.Size.Height * 2);
            var statsChartArea = new ChartArea();
            statsChartArea.Name = "statComparsion";
            newChart.ChartAreas.Add(statsChartArea);
            newChart.Legends.Add(new Legend(statsChartArea.Name));
            newChart.Legends[statsChartArea.Name].DockedToChartArea = statsChartArea.Name;
            newChart.Legends[statsChartArea.Name].IsDockedInsideChartArea = false;

            //create series 
            var mainQuery = (from p in childTasksArr
                    // from c in p.StatMods                               
                    select p.Key.StatMods.First().Stat)
                .Distinct();

            foreach (var stat in mainQuery)
            {
                newChart.Series.Add(stat);
                newChart.Series[stat].ChartArea = statsChartArea.Name;
                newChart.Series[stat].ChartType = SeriesChartType.Line;
                newChart.Series[stat].Legend = statsChartArea.Name;
                newChart.Series[stat].BorderWidth = 3;
                newChart.Series[stat].Points.AddXY(0, 0);
            }

            statsChartArea.AxisX.Minimum = 0;
            statsChartArea.AxisX.Title = "Upgrade";
            statsChartArea.AxisY.Title = "Party DPAV increase(vs normal run)";

            foreach (var subTask in childTasksArr.OrderBy(x => x.Key.UpgradesCount))
            {
                var statMod = subTask.Key.StatMods.First();
                newChart.Series[statMod.Stat].Points
                    .AddXY(subTask.Key.UpgradesCount, subTask.Value.avgDPAV - task.Value.avgDPAV);
                newChart.Series[statMod.Stat].Points.Last().Label = $"wr:{subTask.Value.WinRate:f}%";
                //newChart.Series[statMod.Stat].Points.Last().Label = $"wr:{subtask.Data.WinRate:f}% wcl:{subtask.Data.Cycles:f} dcl:{subtask.Data.DefeatCycles:f}";// extended stats
            }
        }

        GuiUtils.ApplyDarkLightTheme(newChart);
        return newChart;
    }
}