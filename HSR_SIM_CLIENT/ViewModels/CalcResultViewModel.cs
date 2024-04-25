using HSR_SIM_CLIENT.ThreadTools;
using HSR_SIM_CLIENT.Utils;

namespace HSR_SIM_CLIENT.ViewModels;

public class CalcResultViewModel(
    KeyValuePair<SimTask, ThreadJob.RAggregatedData> task,
    IEnumerable<KeyValuePair<SimTask, ThreadJob.RAggregatedData>>? child)
{
    public SimTask TaskKey => task.Key;
    public KeyValuePair<SimTask, ThreadJob.RAggregatedData> Task => task;
    public string? PrintName => $"Task(mods:{task.Key?.StatMods?.Count ?? 0})";

    public IEnumerable<CalcResultViewModel>? ChildVm
    {
        get
        {
            if (child is null) return null;
            var res = new List<CalcResultViewModel>();
            foreach (var kv in child) res.Add(new CalcResultViewModel(kv, null));

            return res;
        }
    }

    public IEnumerable<KeyValuePair<SimTask, ThreadJob.RAggregatedData>>? Child => child;
}