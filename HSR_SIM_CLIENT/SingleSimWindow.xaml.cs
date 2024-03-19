using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
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

namespace HSR_SIM_CLIENT;

public partial class SingleSimWindow : INotifyPropertyChanged
{
    private readonly Worker wrk;
    private ObservableCollection<EventViewModel> events;
    private EventViewModel? selectedEvent;
    private ObservableCollection<Team> teams;


    public SingleSimWindow(SimCls simCls, string? devModePath = null, bool chkDevMode = false)
    {
        InitializeComponent();
        wrk = new Worker();
        wrk.CbRend += WorkerCallBackImages;
        wrk.CbGetDecision = WorkerCallBackGetDecision;
        wrk.DevMode = chkDevMode;
        wrk.LoadScenarioFromSim(simCls, devModePath);
        teams = new ObservableCollection<Team>();
        events = new ObservableCollection<EventViewModel>();
    }


    public ObservableCollection<Team> Teams
    {
        get => teams;
        private set
        {
            if (Equals(value, teams)) return;
            teams = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<EventViewModel> Events
    {
        get => events;
        private set
        {
            if (Equals(value, events)) return;
            events = value;
            OnPropertyChanged();
        }
    }

    public ICloneable? SelectedObject { get; set; }

    public EventViewModel? SelectedEvent
    {
        get => selectedEvent;
        set
        {
            if (Equals(value, selectedEvent)) return;
            selectedEvent = value;

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
        wrk.MoveStep();
        GetTeamsAndEvents();
    }

    /// <summary>
    ///     read current teams and events from step
    /// </summary>
    private void GetTeamsAndEvents()
    {
        //save selected unit
        Teams.Clear();
        foreach (var team in wrk.Sim.Teams)
            Teams.Add(team);

        //render selected unit

        NotifyPropertyChanged(nameof(Teams));

        //save selected unit
        Events.Clear();
        foreach (var ent in wrk.Sim.CurrentStep.Events)
            Events.Add(new EventViewModel(ent));

        //render selected unit

        NotifyPropertyChanged(nameof(Events));
    }

    private void BtnPrev_OnClick(object sender, RoutedEventArgs e)
    {
        wrk.MoveStep(true);
        GetTeamsAndEvents();
    }


    private void TreeView_OnSelectedUnitChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
    }
}