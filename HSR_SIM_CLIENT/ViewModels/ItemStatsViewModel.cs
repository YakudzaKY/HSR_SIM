using HSR_SIM_CLIENT.Models;

namespace HSR_SIM_CLIENT.ViewModels;

/// <summary>
///     item stats vm
/// </summary>
/// <param name="model"></param>
public class ItemStatsViewModel(ItemStatsModel model)
{
    public string? MainStatName
    {
        get => model.MainStat;
        set => model.MainStat = value;
    }

    public string SubStatName1
    {
        get => model.SecondStats[0].Stat;
        set => model.SecondStats[0].Stat = value;
    }

    public string SubStatName2
    {
        get => model.SecondStats[1].Stat;
        set => model.SecondStats[1].Stat = value;
    }

    public string SubStatName3
    {
        get => model.SecondStats[2].Stat;
        set => model.SecondStats[2].Stat = value;
    }

    public string SubStatName4
    {
        get => model.SecondStats[3].Stat;
        set => model.SecondStats[3].Stat = value;
    }

    public string SubStatVal1
    {
        get => model.SecondStats[0].Val;
        set => model.SecondStats[0].Val = value;
    }

    public string SubStatVal2
    {
        get => model.SecondStats[1].Val;
        set => model.SecondStats[1].Val = value;
    }

    public string SubStatVal3
    {
        get => model.SecondStats[2].Val;
        set => model.SecondStats[2].Val = value;
    }

    public string SubStatVal4
    {
        get => model.SecondStats[3].Val;
        set => model.SecondStats[3].Val = value;
    }
}