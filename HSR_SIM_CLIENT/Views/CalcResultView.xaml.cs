using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using HSR_SIM_CLIENT.ChartTools;
using HSR_SIM_CLIENT.ThreadTools;
using HSR_SIM_CLIENT.Utils;
using HSR_SIM_CLIENT.ViewModels;
using HSR_SIM_CLIENT.Windows;
using HSR_SIM_LIB;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;
using TreeView = System.Windows.Controls.TreeView;
using UserControl = System.Windows.Controls.UserControl;

namespace HSR_SIM_CLIENT.Views;

public sealed partial class CalcResultView() : UserControl, INotifyPropertyChanged
{
    private CalcResultViewModel? selectedTask;
    public CalcResultViewModel ViewModel { get; } = null!;

    private CalcResultViewModel? SelectedTask
    {
        get => selectedTask;
        set
        {
            if (Equals(value, selectedTask)) return;
            selectedTask = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatMods));
        }
    }

    public List<Worker.RStatMod> StatMods => (SelectedTask is null) ? [] : SelectedTask.TaskKey.StatMods;

    public CalcResultView(KeyValuePair<SimTask, ThreadJob.RAggregatedData> task,
        IEnumerable<KeyValuePair<SimTask, ThreadJob.RAggregatedData>>? child) : this()
    {
        ViewModel = new CalcResultViewModel(task, child);
        InitializeComponent();
        RedrawChart(task, child);
    }

    private void TreeView_OnSelectedTaskChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // on child click
        if (((TreeView)sender).SelectedItem is CalcResultViewModel fm)

        {
            if (SelectedTask != fm)
            {
                SelectedTask = fm;
                RedrawChart(SelectedTask.Task, null);
            }
        }
        //on parent click
        else if (((TreeView)sender).SelectedItem is TreeViewItem itm)
        {
            if (SelectedTask != (CalcResultViewModel)itm.Header)
            {
                SelectedTask = (CalcResultViewModel)itm.Header;
                RedrawChart(SelectedTask.Task, SelectedTask.Child);
            }
        }
        else
            SelectedTask = null;
    }

    /// <summary>
    /// redraw chart by Task data
    /// </summary>
    /// <param name="task"></param>
    /// <param name="taskChild"></param>
    private void RedrawChart(KeyValuePair<SimTask, ThreadJob.RAggregatedData> task,
        IEnumerable<KeyValuePair<SimTask, ThreadJob.RAggregatedData>>? taskChild)
    {
        var keyValuePairs = taskChild as KeyValuePair<SimTask, ThreadJob.RAggregatedData>[] ?? taskChild?.ToArray();
        WinHst.Children.Clear();

        WinHst.Children.Add(keyValuePairs?.Count() == 1
            ? new DamageChart(task, keyValuePairs.First())
            : new DamageChart(task, null));

        if (keyValuePairs?.Count() > 1)
        {
            WinHst.Children.Add(new SubTaskDamageChart(task, keyValuePairs));
        }
       
           
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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

    private void BtnSingleSim_OnClick(object sender, RoutedEventArgs e)
    {
        if (SelectedTask != null)
        {
            var singleSimWindow = new SingleSimWindow(SelectedTask.TaskKey.SimScenario, chkDevMode: false,
                statMods: SelectedTask.TaskKey.StatMods);

            singleSimWindow.Show();
        }
    }
}