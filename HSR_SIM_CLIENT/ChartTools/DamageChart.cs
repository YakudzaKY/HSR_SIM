using System.Globalization;
using HSR_SIM_CLIENT.ThreadTools;
using HSR_SIM_CLIENT.Utils;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using ScottPlot;
using ScottPlot.Control;
using ScottPlot.Palettes;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using ScottPlot.WPF;
using Color = ScottPlot.Color;

namespace HSR_SIM_CLIENT.ChartTools;

/// <summary>
///     damage chart class (DPAV)
/// </summary>
public class DamageChart : WpfPlot
{
    /// <summary>
    ///     draw damage chart.
    /// </summary>
    /// <param name="task">Primary task</param>
    /// <param name="offTask">Secondary task</param>
    public DamageChart(KeyValuePair<SimTask, ThreadJob.RAggregatedData> task,
        KeyValuePair<SimTask, ThreadJob.RAggregatedData>? offTask)
    {
        //arrays for error bar
        double[] errorsX = [];
        double[] errorsY = [];
        double[] errorsXNegative = [];
        double[] errorsXPositive = [];
        Bar?[] damageBars = [];
        //unit names for axis 
        Tick[] partyList = [];

        void AddPartyBar(int ndx, ThreadJob.RAggregatedData data, Color color, string label)
        {
            Array.Resize(ref partyList, partyList.Length + 1);
            //party overall bars
            Bar? partyBar = new()
                { Position = ndx, ValueBase = 0, Value = data.avgDPAV, FillColor = color };

            //errror bar
            Array.Resize(ref errorsY, errorsY.Length + 1);
            errorsY[^1] = ndx;
            Array.Resize(ref errorsX, errorsX.Length + 1);
            errorsX[^1] = data.avgDPAV;
            Array.Resize(ref errorsXNegative, errorsXNegative.Length + 1);
            errorsXNegative[^1] = data.avgDPAV - data.minDPAV ?? 0;
            Array.Resize(ref errorsXPositive, errorsXPositive.Length + 1);
            errorsXPositive[^1] = data.maxDPAV - data.avgDPAV;
            partyBar.Label = data.avgDPAV.ToString(CultureInfo.InvariantCulture);
            Array.Resize<Bar>(ref damageBars, damageBars.Length + 1);
            damageBars[^1] = partyBar;
            partyList[^1] = new Tick(ndx, label);
        }

        var title =
            $"{task.Key.SimScenario.CurrentScenario.Name}";
        var ann =
            $"Win ratio:{task.Value.WinRate:f}% Win stats: cycles:{task.Value.Cycles:f} totalAV:{task.Value.TotalAV:f}";
        if (task.Value.WinRate < 100)
            ann += $" Defeat stats: cycles:{task.Value.DefeatCycles:f}";
        ((Interaction)Interaction).Actions = PlotUtils.NoWheelZoom();
        Plot.Title(title);
        Dark palette = new();
        Plot.Axes.Margins(left: 0);


        var units = task.Value.PartyUnits.OrderBy(x => x.Value.avgDPAV).ToArray();

        Array.Resize(ref partyList, units.Length);
        //damage Types dictionary
        Type[] dmgTypes =
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
                unitBar = new Bar
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
            errorsXNegative[^1] = unit.Value.avgDPAV - unit.Value.minDPAV ?? 0;
            Array.Resize(ref errorsXPositive, errorsXPositive.Length + 1);
            errorsXPositive[^1] = unit.Value.maxDPAV - unit.Value.avgDPAV;


            i++;
        }

        AddPartyBar(i, task.Value, palette.Colors[6], "OVERALL");
        if (offTask != null) AddPartyBar(i + 1, offTask.Value.Value, palette.Colors[7], "New Gear Overall");


        var plotDmgBars = Plot.Add.Bars(damageBars);
        plotDmgBars.Horizontal = true;
        Plot.Axes.Left.TickGenerator = new NumericManual(partyList);
        Plot.Axes.Left.MajorTickStyle.Length = 0;


        plotDmgBars.ValueLabelStyle.Bold = true;
        plotDmgBars.ValueLabelStyle.FontSize = 18;
        plotDmgBars.ValueLabelStyle.LineSpacing = 0;
        plotDmgBars.ValueLabelStyle.ForeColor = palette.Colors[7];
        plotDmgBars.Horizontal = true;

        //errors output
        var scatter = Plot.Add.Scatter(errorsX, errorsY);
        scatter.Color = palette.Colors[5];
        scatter.LineStyle.Width = 0;

        ErrorBar eb = new(
            errorsX,
            errorsY,
            xErrorsNegative: errorsXNegative,
            xErrorsPositive: errorsXPositive,
            yErrorsNegative: null,
            yErrorsPositive: null);
        eb.Color = scatter.Color;
        Plot.Add.Plottable(eb);

        //style
        Plot.Legend.IsVisible = true;
        Plot.Legend.Location = Alignment.LowerRight;
        foreach (var dmgType in dmgTypes)
            Plot.Legend.ManualItems.Add(new LegendItem
                { Label = dmgType.Name, FillColor = palette.Colors[Array.IndexOf(dmgTypes, dmgType)] });

        Plot.Legend.ManualItems.Add(new LegendItem { Label = "overall(party mixed)", FillColor = palette.Colors[5] });
        //additional info
        Plot.Add.Annotation(ann, Alignment.UpperRight);
        Plot.Add.Palette = palette;
        Plot.Style.DarkMode();
        Height = 380;
        Refresh();
    }

    public sealed override void Refresh()
    {
        base.Refresh();
    }
}