using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using HSR_SIM_LIB;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;
using Image = System.Drawing.Image;

namespace HSR_SIM_CLIENT;

public partial class SingleSimWindow : INotifyPropertyChanged
{
    private readonly bool busy = false;
    private ObservableCollection<Team> teams;
    private readonly Worker wrk;

    public SingleSimWindow(SimCls simCls, string? devModePath = null, bool chkDevMode = false)
    {
        InitializeComponent();
        wrk = new Worker();
        wrk.CbLog += WorkerCallBackString;
        wrk.CbRend += WorkerCallBackImages;
        wrk.CbGetDecision = WorkerCallBackGetDecision;
        wrk.DevMode = chkDevMode;
        wrk.LoadScenarioFromSim(simCls, devModePath);
        teams = new ObservableCollection<Team>();
       
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

    public bool DbgMode { get; set; }
    public ICloneable? SelectedObject { get; set; }

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
    ///     For text callback
    /// </summary>
    /// <param name="kv"></param>
    private void WorkerCallBackString(KeyValuePair<string, string> kv)
    {
        if (string.Equals(kv.Key, Constant.MsgLog))
        {
            LogTextBlock.AppendText(kv.Value);
            LogTextBlock.AppendText(Environment.NewLine);
            if (!busy)
                LogTextBlock.ScrollToEnd();
        }
        else if (string.Equals(kv.Key, Constant.MsgDebug))
        {
            if (DbgMode)
            {
                DebugTextBlock.AppendText(kv.Value);
                DebugTextBlock.AppendText(Environment.NewLine);
                if (!busy)
                    DebugTextBlock.ScrollToEnd();
            }
        }
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
        foreach (Team team in wrk.Sim.Teams)
            Teams.Add(team);

        //render selected unit
        
        NotifyPropertyChanged(nameof(Teams));
    }

    private void BtnPrev_OnClick(object sender, RoutedEventArgs e)
    {
        wrk.MoveStep(true);
        GetTeamsAndEvents();
    }

    
    private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        
    }
}