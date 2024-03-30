using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_CLIENT.Utils.GuiUtils;
using static HSR_SIM_CLIENT.StatData;

namespace HSR_SIM_CLIENT.Windows;

/// <summary>
///     Stat calc back
/// </summary>
public partial class StatCalc : INotifyPropertyChanged

{
    /// <summary>
    ///     delimiter for save and load multiple string into one string
    /// </summary>
    private const string Delimiter = "\\";

    private ObservableCollection<SelectedItem> profiles = [];


    private ObservableCollection<SelectedItem> scenarios = [];
    private ObservableCollection<SelectedItem> selectedStats = [];
    private Dictionary<string, string>? statValTable;

    private int thrCnt;


    public StatCalc()
    {
        InitializeComponent();
        //refresh combo box data
        RefreshCb();
        ThrCnt = Math.Max(Environment.ProcessorCount / 2 - 1, 1);
    }

    public ObservableCollection<string> AvailableCharacters { get; } = [];
    public string SelectedCharacterToCalc { get; set; }

    /// <summary>
    ///     chosen stat will be replaced from checked stats in table
    /// </summary>
    public SelectedItem? SelectedStatToReplace { get; set; }

    /// <summary>
    ///     selected profiles to run in sim
    /// </summary>
    public ObservableCollection<SelectedItem> Profiles
    {
        get => profiles;
        private set
        {
            if (Equals(value, profiles)) return;
            profiles = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     selected stats from table to run in sim
    /// </summary>
    public ObservableCollection<SelectedItem> SelectedStats
    {
        get => selectedStats;
        private set
        {
            if (Equals(value, selectedStats)) return;
            selectedStats = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Selected scenarios to run in sim
    /// </summary>
    public ObservableCollection<SelectedItem> Scenarios
    {
        get => scenarios;
        private set
        {
            if (Equals(value, scenarios)) return;
            scenarios = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     every iteration will add value  from table STAT-VAL * this value
    /// </summary>
    public int UpgradesPerIterations { get; set; }

    /// <summary>
    ///     total upgrades steps. Every step will run for some times: iterations*checked_stats*this_value
    /// </summary>
    public int UpgradesIterations { get; set; }

    /// <summary>
    ///     MaxHeight for render tables in form
    /// </summary>
    public double TblMaxHeight { get; set; }

    /// <summary>
    ///     calculating in progress flag
    /// </summary>
    public bool WorkInProgress { get; set; } = false;

    /// <summary>
    ///     Iterations per every job
    /// </summary>
    public int IterationsCnt { get; set; } = 1000;

    /// <summary>
    ///     Simulation threads count
    /// </summary>
    public int ThrCnt
    {
        get => thrCnt;
        set
        {
            if (Equals(value, thrCnt)) return;
            thrCnt = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void ReloadProfileCharacters()
    {
        AvailableCharacters.Clear();
        foreach (var item in Profiles.Where(x => x.IsSelected))
        {
            var units =
                XmlLoader.ExtractPartyFromXml(GetProfilePath() + item.Name);
            foreach (var unit in units.Where(x => !AvailableCharacters.Contains(x.Name)).Select(x => x.Name))
                AvailableCharacters.Add(unit);
        }

        OnPropertyChanged(nameof(AvailableCharacters));
    }

    /// <summary>
    ///     Load table with stat and value per upgrade into form tables
    /// </summary>
    /// <param name="table"></param>
    private void LoadStatTable(Dictionary<string, string> table)
    {
        SelectedStats.Clear();
        //save selected table reference(will take values from it before sim run)
        statValTable = table;

        foreach (var item in table) SelectedStats.Add(new SelectedItem(item.Key));

        OnPropertyChanged(nameof(SelectedStats));
    }

    /// <summary>
    ///     Number text box
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }


    /// <summary>
    ///     save checked items to ini
    /// </summary>
    private void SaveIni()
    {
        IniF.IniWriteValue(GetType().Name, nameof(Scenarios),
            Scenarios.Where(x => x.IsSelected).Aggregate("", (current, item) => current + item.Name + Delimiter));
        IniF.IniWriteValue(GetType().Name, nameof(Profiles),
            Profiles.Where(x => x.IsSelected).Aggregate("", (current, item) => current + item.Name + Delimiter));
    }


    /// <summary>
    ///     Refresh combo box
    /// </summary>
    private void RefreshCb()
    {
        //find scenarios
        var files = Directory.GetFiles(GetScenarioPath(), "*.xml");
        Scenarios.Clear();
        foreach (var t in files) Scenarios.Add(new SelectedItem(Path.GetFileName(t)) { IsSelected = false });

        //load checked items from ini
        foreach (var item in IniF.IniReadValue(GetType().Name, nameof(Scenarios)).Split(Delimiter)
                     .Where(x => !string.IsNullOrEmpty(x)))
        {
            var selItem = Scenarios.FirstOrDefault(x => x.Name.Equals(item));
            if (selItem != null)
                selItem.IsSelected = true;
        }

        NotifyPropertyChanged(nameof(Scenarios));

        //parse profiles
        Profiles.Clear();
        Directory.CreateDirectory(GetProfilePath());
        files = Directory.GetFiles(GetProfilePath(), "*.xml");

        foreach (var t in files) Profiles.Add(new SelectedItem(Path.GetFileName(t)) { IsSelected = false });

        //load checked items from ini file
        foreach (var item in IniF.IniReadValue(GetType().Name, nameof(Profiles)).Split(Delimiter)
                     .Where(x => !string.IsNullOrEmpty(x)))
        {
            var selItem = Profiles.FirstOrDefault(x => x.Name.Equals(item));
            if (selItem != null)
                selItem.IsSelected = true;
        }

        NotifyPropertyChanged(nameof(Profiles));

        ReloadProfileCharacters();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void StatCalc_OnClosing(object? sender, CancelEventArgs e)
    {
        //save ini on form close 
        SaveIni();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        //also save ini before run sim(if got freeze or app crash cause out of memory) 
        SaveIni();
    }

    private void BtnSubStats_OnClick(object sender, RoutedEventArgs e)
    {
        //Load data from subStats into form tables
        UpgradesIterations = 4;
        NotifyPropertyChanged(nameof(UpgradesIterations));
        UpgradesPerIterations = 4;
        NotifyPropertyChanged(nameof(UpgradesPerIterations));
        LoadStatTable(SubStatsUpgrades);
    }

    private void BtnMainStats_OnClick(object sender, RoutedEventArgs e)
    {
        //Load data from mainStats into form tables
        UpgradesIterations = 1;
        NotifyPropertyChanged(nameof(UpgradesIterations));
        UpgradesPerIterations = 15;
        NotifyPropertyChanged(nameof(UpgradesPerIterations));
        LoadStatTable(MainStatsUpgrades);
    }

    private void StatCalc_OnLoaded(object sender, RoutedEventArgs e)
    {
        //determine max table height to render in form
        TblMaxHeight = tbCalcType.ActualHeight - tbiStatImpact.ActualHeight - LblSelStats.ActualHeight;
        NotifyPropertyChanged(nameof(TblMaxHeight));
    }

    /// <summary>
    ///     On profile check box check
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ToggleProfileButton_OnChecked(object sender, RoutedEventArgs e)
    {
        ReloadProfileCharacters();
    }

    /// <summary>
    ///     item with check box
    /// </summary>
    /// <param name="Name"></param>
    public record SelectedItem(string Name)
    {
        public bool IsSelected { get; set; }
    }
}