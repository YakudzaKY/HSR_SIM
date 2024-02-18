using System.IO;
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
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_CLIENT.GuiUtils;

namespace HSR_SIM_CLIENT;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public string[] Profiles { get; set; } 
    public string[] Sceneraios { get; set; }
    private BindingSource bindProfiles,bindScenarios;
    public MainWindow()
    {
        InitializeComponent();
        bindProfiles = new BindingSource();
        bindScenarios = new BindingSource();
        CbProfile.ItemsSource = bindProfiles;
        CbScenario.ItemsSource = bindScenarios;
        RefreshCB();
    }
    
    private void RefreshCB()
    {
        var files = Directory.GetFiles(GetScenarioPath(),"*.xml");
        Sceneraios = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            Sceneraios[i]=Path.GetFileName(files[i]);
        }


        Directory.CreateDirectory(GetProfilePath());
        files = Directory.GetFiles(GetProfilePath(),"*.xml");
        Profiles = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            Profiles[i]=Path.GetFileName(files[i]);
        }
        
        bindProfiles.DataSource = Profiles;
        bindScenarios.DataSource = Sceneraios ;

    }
    private void BtnLoad_OnClick(object sender, RoutedEventArgs e)
    {
        SimCls SimScenario = XMLLoader.LoadCombatFromXml(GetScenarioPath() + CbScenario.Text, GetProfilePath() + (string)CbProfile.Text);
        
    }

    private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshCB();
    }
}