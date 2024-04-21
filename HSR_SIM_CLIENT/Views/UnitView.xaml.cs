using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using HSR_SIM_CLIENT.ViewModels;

namespace HSR_SIM_CLIENT.Views;

public partial class UnitView : INotifyPropertyChanged
{
    public static readonly DependencyProperty UnitToViewProperty;


    static UnitView()
    {
        UnitToViewProperty = DependencyProperty.Register(nameof(UnitToView), typeof(UnitViewModel),
            typeof(UnitView), new FrameworkPropertyMetadata(PropChangedCb));
    }

    public UnitView()
    {
        InitializeComponent();
    }

    public UnitViewModel UnitToView
    {
        get => (UnitViewModel)GetValue(UnitToViewProperty);
        set => SetValue(UnitToViewProperty, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static void PropChangedCb(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UnitView bv)
            bv.RefreshData();
    }

    private void RefreshData()
    {
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
}