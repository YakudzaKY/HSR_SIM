using System.ComponentModel;
using System.Runtime.CompilerServices;
using HSR_SIM_CLIENT.Utils;

namespace HSR_SIM_CLIENT.Models;

/// <summary>
/// class for ItemStats control
/// </summary>
public sealed class ItemStatsModel:INotifyPropertyChanged
{
    public record StatValRec(string Val = "", string Stat = "")
    {
        public string Stat { get; set; } = Stat;
        public string Val { get; set; } = Val;
    }

    public string? MainStat { get; set; } = string.Empty;

    public StatValRec[] SecondStats { get; } = new[]
        { new StatValRec(), new StatValRec(), new StatValRec(), new StatValRec() };

    //reset values
    private void Clear()
    {
        MainStat = string.Empty;
        foreach (var stat in SecondStats)
        {
            stat.Stat = string.Empty;
            stat.Val = string.Empty;
        }
    }

    /// <summary>
    /// fill stats from array
    /// </summary>
    /// <param name="items"></param>
    public void FillStats(IEnumerable<KeyValuePair<int, OcrUtils.RStatWordRec>> items)
    {
        Clear();
        int i = 0;
        foreach (var item in items)
        {
            //main stat
            if (i == 0)
            {
                MainStat = item.Value.Key;
            }
            //substats
            else if (i > 0)
            {
                SecondStats[i - 1].Stat = item.Value.Key;
                SecondStats[i - 1].Val = item.Value.Value;
            }

            i++;
        }
        NotifyPropertyChanged(nameof(SecondStats));
        NotifyPropertyChanged(nameof(MainStat));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }


    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}