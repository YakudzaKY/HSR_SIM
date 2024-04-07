using HSR_SIM_CLIENT.ThreadTools;
using HSR_SIM_CLIENT.Utils;
using HSR_SIM_CLIENT.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace HSR_SIM_CLIENT.Views;

public partial class CalcResultView ():UserControl
{
    private CalcResultViewModel viewModel = null!;

    public KeyValuePair<SimTask, ThreadJob.RAggregatedData> Task { get; set; } 

    public CalcResultViewModel ViewModel
    {
        get
        {
            return viewModel ??= new CalcResultViewModel(Task);
        }
    }
    public CalcResultView (KeyValuePair<SimTask, ThreadJob.RAggregatedData> task) : this()
    {
        Task = task;
        InitializeComponent();
    }
}