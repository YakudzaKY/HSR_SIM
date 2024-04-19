using HSR_SIM_CLIENT.ThreadTools;
using HSR_SIM_CLIENT.Utils;
using ScottPlot;
using ScottPlot.Control;
using ScottPlot.WPF;
using Color = ScottPlot.Color;

namespace HSR_SIM_CLIENT.ChartTools;
/// <summary>
/// chart rendering sub-task results
/// </summary>
public class SubTaskDamageChart:WpfPlot
{

    private record PointsRec( KeyValuePair<SimTask, ThreadJob.RAggregatedData> Task )
    {
        //x
        public List<double> XS { get; } = [0];
        //y
        public List<double> YS { get;  } = [0];
        //win ratio
        public List<double> WS { get;  } = [Task.Value.WinRate];
        
    }
    
    public SubTaskDamageChart( KeyValuePair<SimTask, ThreadJob.RAggregatedData> task,
        IEnumerable<KeyValuePair<SimTask, ThreadJob.RAggregatedData>> childTasks)
    {
        var childTasksArr = childTasks as KeyValuePair<SimTask, ThreadJob.RAggregatedData>[] ?? childTasks.ToArray();
        Plot.Title("Sub-task DPAV comparison");
        ScottPlot.Palettes.Dark palette = new();
        Plot.Axes.Margins(left: 0);
        ((Interaction)Interaction).Actions = PlotUtils.NoWheelZoom();
        Plot.Axes.Left.Label.Text = "DPAV Delta";
        Plot.Axes.Bottom.Label.Text = "Upgrades count";
 

        Dictionary<string, PointsRec> points= new Dictionary<string, PointsRec>();
        //get every task and generate point
        foreach (var subTask in childTasksArr.OrderBy(x => x.Key.UpgradesCount))
        {
            
            var statMod = subTask.Key.StatMods.First();
            
            if (!points.ContainsKey(statMod.Stat))
            {
                points[statMod.Stat] = new PointsRec(task);
            }
            points[statMod.Stat].XS.Add(subTask.Key.UpgradesCount); 
            points[statMod.Stat].YS.Add(subTask.Value.avgDPAV - task.Value.avgDPAV); 
            points[statMod.Stat].WS.Add(subTask.Value.WinRate); 
        }

        int i = 0;
        foreach (var dictRec in points)
        {
            Color color = palette.Colors[i];
            Plot.Add.Scatter(dictRec.Value.XS, dictRec.Value.YS).Color =color;
            for (int j = 0; j < dictRec.Value.XS.Count(); j++)
            {
                Plot.Add.Text($"wr:{dictRec.Value.WS[j]}%", dictRec.Value.XS[j], dictRec.Value.YS[j]).Color=color;
            }
           
            Plot.Legend.ManualItems.Add(new()
                { Label = dictRec.Key, FillColor = color });
            i++;
            if (i > 7)
                i = 0;
        }
        
        //style
        Plot.Legend.IsVisible = true;
        Plot.Legend.Location = Alignment.LowerRight;
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