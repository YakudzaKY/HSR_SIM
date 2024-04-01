using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using HSR_SIM_CLIENT.ViewModels;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Utils;
using TreeView = System.Windows.Controls.TreeView;


namespace HSR_SIM_CLIENT.Views;

public partial class BuffView : INotifyPropertyChanged
{
    public static readonly DependencyProperty BuffToViewProperty;
    

    static BuffView()
    {
        BuffToViewProperty = DependencyProperty.Register(nameof(BuffToView), typeof(BuffViewModel),
            typeof(BuffView), new FrameworkPropertyMetadata(PropChangedCb));
        
    }

    private static void PropChangedCb(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BuffView bv)
            bv.RefreshData();
    }
    private void RefreshData()
    {
        SelectedCondition = null;
     
    }


    public BuffViewModel BuffToView
    {
        get => (BuffViewModel)GetValue(BuffToViewProperty);
        set => SetValue(BuffToViewProperty, value);
    }



    public BuffView()
    {
        InitializeComponent();
    }

    private ConditionViewModel? selectedCondition;
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