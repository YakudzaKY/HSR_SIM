using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Serialization;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_CLIENT.GuiUtils;

namespace HSR_SIM_CLIENT;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private string[] scenarios = Array.Empty<string>();
    private string[] profiles = Array.Empty<string>();

    public string[] Profiles
    {
        get => profiles;
 
    }

    public string[] Scenarios
    {
        get => scenarios;
    }

    public bool DevMode { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        RefreshCb();
    }
    
    /// <summary>
    /// Refresh combo box
    /// </summary>
    private void RefreshCb()
    {
        //find scenarios
        var files = Directory.GetFiles(GetScenarioPath(),"*.xml");
        Array.Resize( ref scenarios, files.Length);
        for (int i = 0; i < files.Length; i++)
        {
            scenarios[i]=Path.GetFileName(files[i]);
        }
        NotifyPropertyChanged("Scenarios");

        //parse profiles
        Directory.CreateDirectory(GetProfilePath());
        files = Directory.GetFiles(GetProfilePath(),"*.xml");
        Array.Resize( ref profiles, files.Length);
        for (int i = 0; i < files.Length; i++)
        {
            profiles[i]=Path.GetFileName(files[i]);
        }
        NotifyPropertyChanged("Profiles");

        //select saved options
        CbScenario.SelectedIndex = CbScenario.Items.IndexOf(IniF.IniReadValue(GetType().Name, CbScenario.Name));
        CbProfile.SelectedIndex = CbProfile.Items.IndexOf(IniF.IniReadValue(GetType().Name, CbProfile.Name));
        string boolVal = IniF.IniReadValue(GetType().Name, "DevMode");
        DevMode = !String.IsNullOrEmpty(boolVal) && bool.Parse(boolVal);
        NotifyPropertyChanged("DevMode");

    }
    private void BtnLoad_OnClick(object sender, RoutedEventArgs e)
    {
        //save to prevent data loss if app crash or freeze
        SaveIni();
        SingleSimWindow singleSimWindow = new SingleSimWindow();
        singleSimWindow.SetSim(XMLLoader.LoadCombatFromXml(GetScenarioPath() + CbScenario.Text, GetProfilePath() + (string)CbProfile.Text),chkDevMode:DevMode);
        singleSimWindow.Show();

    }

    private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshCb();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this,new PropertyChangedEventArgs(name));
        }
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
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
        IniF.IniWriteValue(GetType().Name, "DevMode", DevMode.ToString());
    }
    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        SaveIni();
    }
}