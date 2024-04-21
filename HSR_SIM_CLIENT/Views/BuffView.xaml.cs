using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using HSR_SIM_CLIENT.ViewModels;
using TreeView = System.Windows.Controls.TreeView;


namespace HSR_SIM_CLIENT.Views;

public partial class BuffView : INotifyPropertyChanged
{
    public static readonly DependencyProperty BuffToViewProperty;

    private ConditionViewModel? selectedCondition;


    static BuffView()
    {
        BuffToViewProperty = DependencyProperty.Register(nameof(BuffToView), typeof(BuffViewModel),
            typeof(BuffView), new FrameworkPropertyMetadata(PropChangedCb));
    }


    public BuffView()
    {
        InitializeComponent();
    }


    public BuffViewModel BuffToView
    {
        get => (BuffViewModel)GetValue(BuffToViewProperty);
        set => SetValue(BuffToViewProperty, value);
    }

    public ConditionViewModel? SelectedCondition
    {
        get => selectedCondition;
        set
        {
            if (Equals(value, selectedCondition)) return;
            selectedCondition = value;

            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static void PropChangedCb(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BuffView bv)
            bv.RefreshData();
    }

    private void RefreshData()
    {
        SelectedCondition = null;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void TreeView_OnSelectedConditionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (((TreeView)sender).SelectedItem is ConditionViewModel cvm)

            SelectedCondition = cvm;
    }
}