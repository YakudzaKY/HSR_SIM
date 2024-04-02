namespace HSR_SIM_CLIENT.Models;

/// <summary>
/// class for ItemStats control
/// </summary>
public class ItemStatsModel
{
    public record StatValRec(string Val= "", string Stat = "")
    {
        public string Stat { get; set; } = Stat;
        public string Val { get; set; } = Val;
    }

    public string? MainStat { get; set; } = string.Empty;
    public StatValRec[] SecondStats { get; } = new []{new StatValRec(),new StatValRec(),new StatValRec(),new StatValRec()};
}