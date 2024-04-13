using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using HSR_SIM_CLIENT.ChartTools;
using HSR_SIM_CLIENT.Models;
using HSR_SIM_CLIENT.ThreadTools;
using HSR_SIM_CLIENT.Utils;
using HSR_SIM_CLIENT.Views;
using HSR_SIM_LIB;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_CLIENT.Utils.GuiUtils;
using static HSR_SIM_CLIENT.Utils.StatData;


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

    /// <summary>
    ///     force new OCR rectangles
    /// </summary>
    private bool forceNewRect;

    private bool interruptFlag;
    private ThreadJob? threadJob;
    private AggregateThread? aggThread;
    private List<SimTask>? myTaskList;
    private ObservableCollection<SelectedItem> profiles = [];


    private ObservableCollection<SelectedItem> scenarios = [];
    private ObservableCollection<SelectedItem> selectedStats = [];
    private Dictionary<string, string>? statValTable;


    public ItemStatsModel ItemStatsUnequipped { get; } = new ItemStatsModel();
    public ItemStatsModel ItemStatsEquipped { get; } = new ItemStatsModel();

    public StatCalc()
    {
        //refresh combo box data
        LoadFromIni();
        InitializeComponent();
        if (ThrCnt == 0)
            ThrCnt = Math.Max(Environment.ProcessorCount / 2 - 1, 1);
    }

    public ObservableCollection<string> AvailableCharacters { get; } = [];
    public ObservableCollection<string> AvailableLocalizations { get; } = [];
    
    public string? SelectedLocalization
    {
        get => selectedLocalization;
        set
        {
            if (Equals(value, selectedLocalization)) return;
            selectedLocalization = value;
            OnPropertyChanged();
        }
    }
    
    public string? SelectedCharacterToCalc
    {
        get => selectedCharacterToCalc;
        set
        {
            if (Equals(value, selectedCharacterToCalc)) return;
            selectedCharacterToCalc = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Tab selection stat calc
    /// </summary>
    /// <returns></returns>
    private bool statImpactTabSelected = true;

    public bool StatImpactTabSelected
    {
        get => statImpactTabSelected;
        set
        {
            if (Equals(value, statImpactTabSelected)) return;
            statImpactTabSelected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Tab selection stat calc
    /// </summary>
    /// <returns></returns>
    private bool gearReplaceTabSelected = true;

    public bool GearReplaceTabSelected
    {
        get => gearReplaceTabSelected;
        set
        {
            if (Equals(value, gearReplaceTabSelected)) return;
            gearReplaceTabSelected = value;
            OnPropertyChanged();
        }
    }

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
    ///     calculating in progress flag
    /// </summary>
    private bool workInProgress;

    public bool WorkInProgress
    {
        get => workInProgress;
        private set
        {
            if (Equals(value, workInProgress)) return;
            workInProgress = value;

            OnPropertyChanged();
        }
    }


    /// <summary>
    ///     calculating in progress flag
    /// </summary>
    private string? workProgressText;

    public string? WorkProgressText
    {
        get => workProgressText;
        private set
        {
            if (Equals(value, workProgressText)) return;
            workProgressText = value;

            OnPropertyChanged();
        }
    }

    private int simOperationsMax;

    public int SimOperationsMax
    {
        get => simOperationsMax;
        private set
        {
            if (Equals(value, simOperationsMax)) return;
            simOperationsMax = value;

            OnPropertyChanged();
        }
    }

    private int simOperationsCurrent;
    private string? selectedCharacterToCalc;
    private string? selectedLocalization;

    public int SimOperationsCurrent
    {
        get => simOperationsCurrent;
        private set
        {
            if (Equals(value, simOperationsCurrent)) return;
            simOperationsCurrent = value;

            OnPropertyChanged();
        }
    }


    /// <summary>
    ///     Iterations per every job
    /// </summary>
    private int iterationsCnt = 1000;

    public int IterationsCnt
    {
        get => iterationsCnt;
        set
        {
            if (Equals(value, iterationsCnt)) return;
            iterationsCnt = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Simulation threads count
    /// </summary>
    private int thrCnt;

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

        IniF.IniWriteValue(GetType().Name, nameof(ItemStatsEquipped) + "Sub",
            ItemStatsEquipped.SecondStats.Aggregate("",
                (current, statVal) => current + statVal.Stat + "=" + statVal.Val + Delimiter));
        IniF.IniWriteValue(GetType().Name, nameof(ItemStatsEquipped) + "Main", ItemStatsEquipped.MainStat);

        IniF.IniWriteValue(GetType().Name, nameof(ItemStatsUnequipped) + "Sub",
            ItemStatsUnequipped.SecondStats.Aggregate("",
                (current, statVal) => current + statVal.Stat + "=" + statVal.Val + Delimiter));
        IniF.IniWriteValue(GetType().Name, nameof(ItemStatsUnequipped) + "Main", ItemStatsUnequipped.MainStat);

        IniF.IniWriteValue(GetType().Name, nameof(SelectedCharacterToCalc), SelectedCharacterToCalc);
        IniF.IniWriteValue(GetType().Name, nameof(SelectedLocalization), SelectedLocalization);

        IniF.IniWriteValue(GetType().Name, nameof(StatImpactTabSelected), StatImpactTabSelected.ToString().ToLower());
        IniF.IniWriteValue(GetType().Name, nameof(GearReplaceTabSelected), GearReplaceTabSelected.ToString().ToLower());
        IniF.IniWriteValue(GetType().Name, nameof(ThrCnt), ThrCnt.ToString());
        IniF.IniWriteValue(GetType().Name, nameof(IterationsCnt), IterationsCnt.ToString());
    }


    /// <summary>
    ///     Refresh combo box
    /// </summary>
    private void LoadFromIni()
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

        //localizations
        AvailableLocalizations.Clear();
        files = Directory.GetFiles(OcrUtils.GetTessDataFolder(), "*.traineddata");
        foreach (var t in files) AvailableLocalizations.Add((Path.GetFileNameWithoutExtension(t)));
        NotifyPropertyChanged(nameof(AvailableLocalizations));
        
        //load  item stats
        var i = 0;
        ItemStatsEquipped.MainStat = IniF.IniReadValue(GetType().Name, nameof(ItemStatsEquipped) + "Main");
        foreach (var item in IniF.IniReadValue(GetType().Name, nameof(ItemStatsEquipped) + "Sub").Split(Delimiter)
                     .Where(x => !string.IsNullOrEmpty(x)))
        {
            string[] keyVal = item.Split('=');
            ItemStatsEquipped.SecondStats[i].Stat = keyVal[0];
            ItemStatsEquipped.SecondStats[i].Val = keyVal[1];
            i++;
        }

        i = 0;
        ItemStatsUnequipped.MainStat = IniF.IniReadValue(GetType().Name, nameof(ItemStatsUnequipped) + "Main");
        foreach (var item in IniF.IniReadValue(GetType().Name, nameof(ItemStatsUnequipped) + "Sub").Split(Delimiter)
                     .Where(x => !string.IsNullOrEmpty(x)))
        {
            string[] keyVal = item.Split('=');
            ItemStatsUnequipped.SecondStats[i].Stat = keyVal[0];
            ItemStatsUnequipped.SecondStats[i].Val = keyVal[1];
            i++;
        }


        SelectedCharacterToCalc = IniF.IniReadValue(GetType().Name, nameof(SelectedCharacterToCalc));
        SelectedLocalization = IniF.IniReadValue(GetType().Name, nameof(SelectedLocalization));
        
        bool.TryParse(IniF.IniReadValue(GetType().Name, nameof(StatImpactTabSelected)), out statImpactTabSelected);
        bool.TryParse(IniF.IniReadValue(GetType().Name, nameof(GearReplaceTabSelected)), out gearReplaceTabSelected);
        NotifyPropertyChanged(nameof(StatImpactTabSelected));
        NotifyPropertyChanged(nameof(GearReplaceTabSelected));

        if (int.TryParse(IniF.IniReadValue(GetType().Name, nameof(ThrCnt)), out var parsedThrCnt))
        {
            ThrCnt = parsedThrCnt;
        }

        ;
        if (int.TryParse(IniF.IniReadValue(GetType().Name, nameof(IterationsCnt)), out var parsedIterationsCnt))
        {
            IterationsCnt = parsedIterationsCnt;
        }

        ;


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
        SaveIni();
        //also save ini before run sim(if got freeze or app crash cause out of memory) 
        //generate task list
        myTaskList = new List<SimTask>();

        foreach (var scenario in Scenarios.Where(x => x.IsSelected))
        foreach (var profile in Profiles.Where(x => x.IsSelected))
        {
            // first parent task
            var prnt = new SimTask
            {
                SimScenario = XmlLoader.LoadCombatFromXml(GetScenarioPath() + scenario.Name,
                    GetProfilePath() + profile.Name)
            };
            myTaskList.Add(prnt);
            //childs
            myTaskList.AddRange(GetStatsSubTasks(scenario.Name, profile.Name, prnt));
        }

        DoCalculations();
    }

    /// <summary>
    /// clear charts->do calc->render charts
    /// </summary>
    private async Task DoCalculations()
    {
        foreach (var item in StackCharts.Children)
        {
            //chart dispose
            ((CalcResultView)item).WinHst.Child.Dispose();
        }

        StackCharts.Children.Clear();
        await Task.Run(DoJob);

        foreach (var task in threadJob.CombatData.Where(x => x.Key.Parent is null))
        {
            //var newChart = ChartUtils.GetChart(task, threadJob.CombatData.Where(x => x.Key.Parent == task.Key));
            //new WindowsFormsHost() { Child = newChart }
            StackCharts.Children.Add(
                new CalcResultView(task, threadJob.CombatData.Where(x => x.Key.Parent == task.Key)));
        }
    }

    /// <summary>
    ///  start and wait threads calculations
    /// </summary>
    private void DoJob()
    {
        WorkInProgress = true;
        threadJob = new ThreadJob(myTaskList, IterationsCnt);


        interruptFlag = false;
        //check four double call this proc
        if (aggThread?.IsAlive ?? false)
            return;


        aggThread = new AggregateThread(threadJob, ThrCnt);


        SimOperationsCurrent = 0;
        SimOperationsMax = myTaskList.Count * threadJob.Iterations;


        aggThread.Start();

        int oldEta = 0;
        do
        {
            var stDate = DateTime.Now;
            if (interruptFlag)
                aggThread.Interrupt();

            var progressBefore = aggThread.Progress();

            Thread.Sleep(1000);

            SimOperationsCurrent = aggThread.Progress();
            var crDate = DateTime.Now;
            var diffInSeconds = (crDate - stDate).TotalSeconds;
            var performance =
                diffInSeconds > 0 ? (SimOperationsCurrent - progressBefore) / diffInSeconds : 0; //sims per sec
            var eta = performance > 0
                ? (int)((SimOperationsMax - SimOperationsCurrent) / performance)
                : 0; //sec estimate 
            //add prev result for smooth graph
            var avgEta = (eta + oldEta) / 2;
            oldEta = eta;
            var etaM = (int)Math.Floor((double)avgEta / 60);
            var etaFormated = $"{etaM}m {avgEta - etaM * 60}s";
            WorkProgressText = $"{SimOperationsCurrent}\\{SimOperationsMax}  {performance:f}\\sec    ETA:{etaFormated}";
        } while (aggThread.IsAlive);


        WorkInProgress = false;
    }

    //get Stat mods by checked item(one mod atm)
    private List<Worker.RStatMod> GetStatMods(string? character, string item, int step, string minusItem = null)
    {
        var res = new List<Worker.RStatMod>();
        res.Add(new Worker.RStatMod
            { Character = character, Stat = item, Val = SearchStatDeltaByName(item) * step });
        if (!string.IsNullOrEmpty(minusItem))
            res.Add(new Worker.RStatMod
            {
                Character = character,
                Stat = minusItem,
                Val = -SearchStatDeltaByName(minusItem) * step
            });
        return res;
    }

    private double SearchStatDeltaByName(string item)
    {
        if (statValTable is null)
            return 0;
        return double.Parse(statValTable[item]);
    }

    //parse stat value 
    private static double ExctractDoubleVal(string inputStr)
    {
        inputStr = inputStr.Replace('.', ',');
        return inputStr.EndsWith('%')
            ? double.Parse(inputStr.Substring(0, inputStr.Length - 1)) / 100
            : double.Parse(inputStr);
    }

    private List<SimTask> GetStatsSubTasks(string scenario, string profile, SimTask simTask)
    {
        var res = new List<SimTask>();
        if (StatImpactTabSelected)
        {
            if (!string.IsNullOrEmpty(SelectedCharacterToCalc))
                foreach (var selectedStat in SelectedStats.Where(x => x.IsSelected))
                    for (var i = 1; i <= UpgradesIterations; i++)
                        res.Add(new SimTask
                        {
                            SimScenario = XmlLoader.LoadCombatFromXml(GetScenarioPath() + scenario,
                                GetProfilePath() + profile),
                            Parent = simTask,
                            UpgradesCount = i * UpgradesPerIterations,
                            StatMods = GetStatMods(SelectedCharacterToCalc, selectedStat.Name,
                                i * UpgradesPerIterations, SelectedStatToReplace?.Name ?? string.Empty)
                        });
        }
        else if (GearReplaceTabSelected)
        {
            if (!string.IsNullOrEmpty(SelectedCharacterToCalc))
            {
                var statModList = new List<Worker.RStatMod>();

                //unequipped
                if (!string.IsNullOrEmpty(ItemStatsUnequipped.MainStat))
                {
                    double val = SearchStatBaseByNameInMainStats(ItemStatsUnequipped.MainStat) +
                                 15 * SearchStatDeltaByNameInMainStats(ItemStatsUnequipped.MainStat);
                    statModList.Add(new Worker.RStatMod
                    {
                        Character = SelectedCharacterToCalc,
                        Stat = ItemStatsUnequipped.MainStat,
                        Val = -1 * val
                    });
                }

                foreach (var itemStat in ItemStatsUnequipped.SecondStats)
                {
                    if (!string.IsNullOrEmpty(itemStat.Stat) && !string.IsNullOrEmpty(itemStat.Val))
                        statModList.Add(new Worker.RStatMod
                        {
                            Character = SelectedCharacterToCalc,
                            Stat = itemStat.Stat,
                            Val = -1 * ExctractDoubleVal(itemStat.Val)
                        });
                }

                //equipped
                if (!string.IsNullOrEmpty(ItemStatsEquipped.MainStat))
                {
                    double val = SearchStatBaseByNameInMainStats(ItemStatsEquipped.MainStat) +
                                 15 * SearchStatDeltaByNameInMainStats(ItemStatsEquipped.MainStat);
                    statModList.Add(new Worker.RStatMod
                    {
                        Character = SelectedCharacterToCalc,
                        Stat = ItemStatsEquipped.MainStat,
                        Val = val
                    });
                }

                foreach (var itemStat in ItemStatsEquipped.SecondStats)
                {
                    if (!string.IsNullOrEmpty(itemStat.Stat) && !string.IsNullOrEmpty(itemStat.Val))
                        statModList.Add(new Worker.RStatMod
                        {
                            Character = SelectedCharacterToCalc,
                            Stat = itemStat.Stat,
                            Val = ExctractDoubleVal(itemStat.Val)
                        });
                }

                if (statModList.Count > 0)
                {
                    statModList.Insert(0, new Worker.RStatMod { Stat = "NEW GEAR" });
                    res.Add(new SimTask
                    {
                        SimScenario = XmlLoader.LoadCombatFromXml(GetScenarioPath() + scenario,
                            GetProfilePath() + profile),
                        StatMods = statModList,
                        UpgradesCount = 1,
                        Parent = simTask
                    });
                }
            }
        }

        return res;
    }

    private static double SearchStatDeltaByNameInMainStats(string item)
    {
        return ExctractDoubleVal(MainStatsUpgrades.First(x => x.Key.Equals(item)).Value);
    }

    private static double SearchStatBaseByNameInMainStats(string item)
    {
        return ExctractDoubleVal(MainStatsBase.First(x => x.Key.Equals(item)).Value);
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

    private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
    {
        //in case of crash app we have saved values

        interruptFlag = true;
    }

    private void BtnImportScreen_OnClick(object sender, RoutedEventArgs e)
    {
        var keyVal =
            new OcrUtils().GetComparisonItemStat(new WindowInteropHelper(this).Handle.ToInt64(), ref forceNewRect,SelectedLocalization);
        ItemStatsUnequipped.FillStats(keyVal.Where(x => x.Value.StatMode == OcrUtils.RectModeEnm.Minus));
        ItemStatsEquipped.FillStats(keyVal.Where(x => x.Value.StatMode == OcrUtils.RectModeEnm.Plus));
        VvItemStatsEquipped.RefreshData();
        VvItemStatsUnequipped.RefreshData();
    }

    private void BtnResetScanArea_OnClick(object sender, RoutedEventArgs e)
    {
        forceNewRect = true;
    }
}