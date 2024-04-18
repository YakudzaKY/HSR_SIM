using System.Globalization;
using HSR_SIM_CLIENT.ThreadTools;
using HSR_SIM_CLIENT.Utils;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using ScottPlot;
using ScottPlot.WPF;


namespace HSR_SIM_CLIENT.ChartTools;

internal static class ChartUtils
{
    public static WpfPlot CreateChart(KeyValuePair<SimTask, ThreadJob.RAggregatedData> task,
        IEnumerable<KeyValuePair<SimTask, ThreadJob.RAggregatedData>>? childTasks)
    {
        var retChart = new WpfPlot();
        string title =
            $"{task.Key.SimScenario.CurrentScenario.Name} wr:{task.Value.WinRate:f}%";
        string ann = $"Win stats: cycles:{task.Value.Cycles:f} totalAV:{task.Value.TotalAV:f}";
        if (task.Value.WinRate < 100)
            ann += ($" Defeat stats: cycles:{task.Value.DefeatCycles:f}");
        var plot = retChart.Plot;
        plot.Title(title);
        ScottPlot.Palettes.Dark palette = new();
        plot.Axes.Margins(left: 0);
        //arrays for error bar
        double[] errorsX = [];
        double[] errorsY = [];
        double[] errorsXNegative = [];
        double[] errorsXPositive = [];
        
        Bar?[] damageBars = [];
        var units = task.Value.PartyUnits.OrderBy(x => x.Value.avgDPAV).ToArray();
        //unit names for axis 
        Tick[] partyList = [];
        Array.Resize(ref partyList, units.Length + 1);
        //damage Types dictionary
        Type[] dmgTypes = new[]
            { typeof(DirectDamage), typeof(DoTDamage), typeof(ToughnessBreak), typeof(ToughnessBreakDoTDamage) };

        var i = 0;
        //per Unit bar
        foreach (var unit in units)
        {
            partyList[i] = new Tick(i, unit.Key);
            double prevVal = 0;
            Bar? unitBar = null;
            foreach (var kindDmg in unit.Value.avgByTypeDPAV.Where(x => x.Value > 0)
                         .OrderBy(x => Array.IndexOf(dmgTypes, x.Key)))
            {
                unitBar = new()
                {
                    Position = i, ValueBase = prevVal, Value = prevVal + kindDmg.Value,
                    FillColor = palette.Colors[Array.IndexOf(dmgTypes, kindDmg.Key)]
                };
                prevVal += kindDmg.Value;
                Array.Resize<Bar>(ref damageBars, damageBars.Length + 1);
                damageBars[^1] = unitBar;
            }

            if (unitBar != null)
                unitBar.Label = unit.Value.avgDPAV.ToString(CultureInfo.InvariantCulture);

            //errror bar
            Array.Resize(ref errorsY, errorsY.Length + 1);
            errorsY[^1] = i;
            Array.Resize(ref errorsX, errorsX.Length + 1);
            errorsX[^1] = prevVal;
            Array.Resize(ref errorsXNegative, errorsXNegative.Length + 1);
            errorsXNegative[^1] = unit.Value.avgDPAV-unit.Value.minDPAV??0;
            Array.Resize(ref errorsXPositive, errorsXPositive.Length + 1);
            errorsXPositive[^1] = unit.Value.maxDPAV-unit.Value.avgDPAV;
         
        
            
            i++;
        }

        //party overall bars
        Bar? partyBar = new()
            { Position = i, ValueBase = 0, Value = task.Value.avgDPAV, FillColor = palette.Colors[6] };
      
        //errror bar
        Array.Resize(ref errorsY, errorsY.Length + 1);
        errorsY[^1] = i;
        Array.Resize(ref errorsX, errorsX.Length + 1);
        errorsX[^1] = task.Value.avgDPAV;
        Array.Resize(ref errorsXNegative, errorsXNegative.Length + 1);
        errorsXNegative[^1] = task.Value.avgDPAV-task.Value.minDPAV??0;
        Array.Resize(ref errorsXPositive, errorsXPositive.Length + 1);
        errorsXPositive[^1] = task.Value.maxDPAV-task.Value.avgDPAV;
        
        partyBar.Label = task.Value.avgDPAV.ToString(CultureInfo.InvariantCulture);
        Array.Resize<Bar>(ref damageBars, damageBars.Length + 1);
        damageBars[^1] = partyBar;
        partyList[i] = new Tick(i, "OVERALL");


        var plotDmgBars = plot.Add.Bars(damageBars);
        plotDmgBars.Horizontal = true;
        plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(partyList);
        plot.Axes.Left.MajorTickStyle.Length = 0;



        plotDmgBars.ValueLabelStyle.Bold = true;
        plotDmgBars.ValueLabelStyle.FontSize = 18;
        plotDmgBars.ValueLabelStyle.LineSpacing = 0;
        plotDmgBars.ValueLabelStyle.ForeColor = palette.Colors[7];
        plotDmgBars.Horizontal = true;

        //errors output
        var scatter = plot.Add.Scatter(errorsX, errorsY);
        scatter.Color = palette.Colors[5];
        scatter.LineStyle.Width = 0;

        ScottPlot.Plottables.ErrorBar eb = new(
            xs: errorsX,
            ys: errorsY,
            xErrorsNegative: errorsXNegative,
            xErrorsPositive: errorsXPositive,
            yErrorsNegative: null,
            yErrorsPositive: null);
        eb.Color = scatter.Color;
        plot.Add.Plottable(eb);
        
        //style
        plot.Legend.IsVisible = true;
        plot.Legend.Location = Alignment.LowerRight;
        foreach (var dmgType in dmgTypes)
        {
            plot.Legend.ManualItems.Add(new()
                { Label = dmgType.Name, FillColor = palette.Colors[Array.IndexOf(dmgTypes, dmgType)] });
        }

        plot.Legend.ManualItems.Add(new() { Label = "Other", FillColor = palette.Colors[5] });
        //additional info
        plot.Add.Annotation(ann, Alignment.UpperRight);
        plot.Add.Palette = palette;
        plot.Style.DarkMode();
        retChart.Height = 380;
        retChart.Refresh();
        
        
        return retChart;
    }
    /*
     *   todo:      //If we have child task-> do additional chart compare with parent results
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
     */

}