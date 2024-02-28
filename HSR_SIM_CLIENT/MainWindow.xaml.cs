using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_CLIENT.GuiUtils;

namespace HSR_SIM_CLIENT;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : INotifyPropertyChanged
{
    private string[] profiles = Array.Empty<string>();
    private string[] scenarios = Array.Empty<string>();

    public MainWindow()
    {
        InitializeComponent();
        RefreshCb();
    }

    public string[] Profiles => profiles;

    public string[] Scenarios => scenarios;

    public bool DevMode { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Refresh combo box
    /// </summary>
    private void RefreshCb()
    {
        //find scenarios
        var files = Directory.GetFiles(GetScenarioPath(), "*.xml");
        Array.Resize(ref scenarios, files.Length);
        for (var i = 0; i < files.Length; i++) scenarios[i] = Path.GetFileName(files[i]);
        NotifyPropertyChanged(nameof(Scenarios));

        //parse profiles
        Directory.CreateDirectory(GetProfilePath());
        files = Directory.GetFiles(GetProfilePath(), "*.xml");
        Array.Resize(ref profiles, files.Length);
        for (var i = 0; i < files.Length; i++) profiles[i] = Path.GetFileName(files[i]);
        NotifyPropertyChanged(nameof(Profiles));

        //select saved options
        CbScenario.SelectedIndex = CbScenario.Items.IndexOf(IniF.IniReadValue(GetType().Name, CbScenario.Name));
        CbProfile.SelectedIndex = CbProfile.Items.IndexOf(IniF.IniReadValue(GetType().Name, CbProfile.Name));
        var boolVal = IniF.IniReadValue(GetType().Name, nameof(DevMode));
        DevMode = !string.IsNullOrEmpty(boolVal) && bool.Parse(boolVal);
        NotifyPropertyChanged(nameof(DevMode));
    }

    private void BtnLoad_OnClick(object sender, RoutedEventArgs e)
    {
        //save to prevent data loss if app crash or freeze
        SaveIni();
        var singleSimWindow = new SingleSimWindow(   XmlLoader.LoadCombatFromXml(GetScenarioPath() + CbScenario.Text, GetProfilePath() + CbProfile.Text),
            chkDevMode: DevMode);
        singleSimWindow.Show();
    }

    private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshCb();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void SaveIni()
    {
        IniF.IniWriteValue(GetType().Name, CbScenario.Name, CbScenario.Text);
        IniF.IniWriteValue(GetType().Name, CbProfile.Name, CbProfile.Text);
        IniF.IniWriteValue(GetType().Name, nameof(DevMode), DevMode.ToString());
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        SaveIni();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        //open stat calc
        new StatCalc().Show();
    }
}