using System.Globalization;
using HSR_SIM_CLIENT.ChartTools;
using HSR_SIM_CLIENT.ThreadTools;
using HSR_SIM_CLIENT.Utils;
using ScottPlot;
using ScottPlot.Control;
using ScottPlot.Palettes;
using ScottPlot.TickGenerators;
using UserControl = System.Windows.Controls.UserControl;

namespace HSR_SIM_CLIENT.Views;

/// <summary>
///     total sims result comparison
/// </summary>
public partial class ChangeGearOverallCompare : UserControl
{
    public ChangeGearOverallCompare(IEnumerable<KeyValuePair<SimTask, ThreadJob.RAggregatedData>> parentData,
        Dictionary<SimTask, ThreadJob.RAggregatedData> threadJobCombatData)
    {
        InitializeComponent();
        var plot = WpfPlot.Plot;
        plot.Title("overall comparison");
        Dark palette = new();
        plot.Axes.Margins(left: 0);
        ((Interaction)WpfPlot.Interaction).Actions = PlotUtils.NoWheelZoom();
        plot.Axes.Bottom.Label.Text = "DPAV Delta";
        plot.Axes.Left.Label.Text = "Scenario+profile";
        plot.Add.Palette = palette;
        plot.Style.DarkMode();
        Bar?[] damageBars = [];
        Tick[] scenarioAndProfiles = [];
        // ReSharper disable once PossibleMultipleEnumeration
        Array.Resize(ref scenarioAndProfiles, parentData.Count());
        Array.Resize(ref damageBars, parentData.Count());
        var i = 0;
        foreach (var item in parentData)
        {
            scenarioAndProfiles[i] = new Tick(i, item.Key.SimScenario.CurrentScenario.ShortName);
            var childTask = threadJobCombatData.FirstOrDefault(x => x.Key.Parent == item.Key);
            //uncompleted sim
            if (childTask.Key == null) return;

            var deltaDpav = childTask.Value.avgDPAV - item.Value.avgDPAV;
            var color = deltaDpav > 0 ? palette.Colors[0] : palette.Colors[1];
            Bar? scenarioProfileBar = new()
                { Position = i, ValueBase = 0, Value = deltaDpav, FillColor = color };

            scenarioProfileBar.Label = deltaDpav.ToString(CultureInfo.InvariantCulture);


            damageBars[i] = scenarioProfileBar;


            i++;
        }


        //style
        plot.Legend.IsVisible = true;
        plot.Legend.Location = Alignment.LowerRight;


        plot.Legend.ManualItems.Add(new LegendItem { Label = "Good :)", FillColor = palette.Colors[0] });
        plot.Legend.ManualItems.Add(new LegendItem { Label = "Bad :(", FillColor = palette.Colors[1] });

        var plotDmgBars = plot.Add.Bars(damageBars!);
        plotDmgBars.Horizontal = true;
        plotDmgBars.ValueLabelStyle.Bold = true;
        plotDmgBars.ValueLabelStyle.FontSize = 18;
        plotDmgBars.ValueLabelStyle.LineSpacing = 0;
        plotDmgBars.ValueLabelStyle.ForeColor = palette.Colors[7];

        plot.Axes.Left.TickGenerator = new NumericManual(scenarioAndProfiles);
        plot.Axes.Left.MajorTickStyle.Length = 0;
    }
}