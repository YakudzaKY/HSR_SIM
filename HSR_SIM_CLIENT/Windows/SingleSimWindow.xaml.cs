using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using HSR_SIM_CLIENT.ViewModels;
using HSR_SIM_LIB;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using Image = System.Drawing.Image;
using TreeView = System.Windows.Controls.TreeView;

namespace HSR_SIM_CLIENT.Windows;

public partial class SingleSimWindow : INotifyPropertyChanged
{
    private readonly Worker _wrk;
    private ObservableCollection<EventViewModel> _events;
    private EventViewModel? _selectedEvent;
    private UnitViewModel? _selectedUnit;
    private ObservableCollection<Team> _teams;


    public SingleSimWindow(SimCls simCls, string? devModePath = null, bool chkDevMode = false,
        List<Worker.RStatMod>? statMods = null)
    {
        InitializeComponent();
        _wrk = new Worker();
        _wrk.CbRend += WorkerCallBackImages;
        _wrk.CbGetDecision = WorkerCallBackGetDecision;
        _wrk.DevMode = chkDevMode;
        _wrk.LoadScenarioFromSim(simCls, devModePath);
        if (statMods != null)
            _wrk.ApplyModes(statMods);
        _teams = new ObservableCollection<Team>();
        _events = new ObservableCollection<EventViewModel>();
    }


    public ObservableCollection<Team> Teams
    {
        get => _teams;
        private set
        {
            if (Equals(value, _teams)) return;
            _teams = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<EventViewModel> Events
    {
        get => _events;
        private set
        {
            if (Equals(value, _events)) return;
            _events = value;
            OnPropertyChanged();
        }
    }

    public UnitViewModel? SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            if (Equals(value, _selectedUnit)) return;
            _selectedUnit = value;

            OnPropertyChanged();
        }
    }


    public EventViewModel? SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            if (Equals(value, _selectedEvent)) return;
            _selectedEvent = value;

            OnPropertyChanged();
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }


    /// <summary>
    ///     ask user what decision are pick from options(do we crit etc...)
    /// </summary>
    /// <param name="items"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    private int WorkerCallBackGetDecision(string[] items, string description)
    {
        var getDecision = new GetDecision(items, description);
        getDecision.ShowDialog();
        return getDecision.ItemIndex;
    }


    private static BitmapImage BitmapToImageSource(Image bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Bmp);
        memory.Position = 0;
        var bitmapToImageSource = new BitmapImage();
        bitmapToImageSource.BeginInit();
        bitmapToImageSource.StreamSource = memory;
        bitmapToImageSource.CacheOption = BitmapCacheOption.OnLoad;
        bitmapToImageSource.EndInit();

        return bitmapToImageSource;
    }

    /// <summary>
    ///     For imagesCallback
    /// </summary>
    /// <param name="combatImg"></param>
    private void WorkerCallBackImages(Bitmap combatImg)
    {
        CombatImg.Source = BitmapToImageSource(combatImg);
    }

    private void BtnNext_OnClick(object sender, RoutedEventArgs e)
    {
        _wrk.MoveStep();
        GetTeamsAndEvents();
    }

    /// <summary>
    ///     read current teams and events from step
    /// </summary>
    private void GetTeamsAndEvents()
    {
        //save selected unit
        Teams.Clear();
        foreach (var team in _wrk.Sim.Teams)
            Teams.Add(team);

        //render selected unit

        NotifyPropertyChanged(nameof(Teams));

        //save selected unit
        Events.Clear();
        if (_wrk.Sim.CurrentStep != null)
            foreach (var ent in _wrk.Sim.CurrentStep.Events)
                Events.Add(new EventViewModel(ent));

        //render selected unit

        NotifyPropertyChanged(nameof(Events));
    }

    private void BtnPrev_OnClick(object sender, RoutedEventArgs e)
    {
        _wrk.MoveStep(true);
        GetTeamsAndEvents();
    }


    private void TreeView_OnSelectedUnitChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // on unit click
        if (((TreeView)sender).SelectedItem is Unit fm)

            SelectedUnit = new UnitViewModel(fm);
        else
            SelectedUnit = null;
    }
}