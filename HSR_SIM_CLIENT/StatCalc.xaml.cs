using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using static HSR_SIM_CLIENT.GuiUtils;

namespace HSR_SIM_CLIENT;

public partial class StatCalc : INotifyPropertyChanged

{
    public bool WorkInProgress { get; set; } = false;
    public int IterationsCnt { get; set; } = 1000;
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

    public StatCalc()
    {
        InitializeComponent();
        profiles = new ObservableCollection<SelectProfile>();
        //refresh combo box data
        RefreshCb();
        ThrCnt=Math.Max((Environment.ProcessorCount/2) - 1,1);
        
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    private string[] scenarios = Array.Empty<string>();
    private ObservableCollection<SelectProfile> profiles;

    public record SelectProfile
    {
        public bool IsSelected { get; set; } = false;
        public string? Name { get; init; }
    }

    public ObservableCollection<SelectProfile> Profiles
    {
        get => profiles;
        private set
        {
            if (Equals(value, profiles)) return;
            profiles = value;
            OnPropertyChanged();
        }
    }

    private void SaveIni()
    {
        IniF.IniWriteValue(GetType().Name, CbScenario.Name, CbScenario.Text);

    }
    public string[] Scenarios => scenarios;


    /// <summary>
    /// Refresh combo box
    /// </summary>
    private void RefreshCb()
    {
        //find scenarios
        var files = Directory.GetFiles(GetScenarioPath(), "*.xml");
        Array.Resize(ref scenarios, files.Length);
        for (var i = 0; i < files.Length; i++)
        {
            scenarios[i] = Path.GetFileName(files[i]);
        }

        NotifyPropertyChanged("Scenarios");

        //parse profiles
        //Profiles.Clear();
        Directory.CreateDirectory(GetProfilePath());
        files = Directory.GetFiles(GetProfilePath(), "*.xml");

        foreach (var t in files)
        {
            Profiles.Add(new SelectProfile() { IsSelected = false, Name = Path.GetFileName(t) });
        }
        NotifyPropertyChanged("Profiles");

        //select saved options
        CbScenario.SelectedIndex = CbScenario.Items.IndexOf(IniF.IniReadValue(GetType().Name, CbScenario.Name));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    private void StatCalc_OnClosing(object? sender, CancelEventArgs e)
    {
        SaveIni();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        SaveIni();
    }
}