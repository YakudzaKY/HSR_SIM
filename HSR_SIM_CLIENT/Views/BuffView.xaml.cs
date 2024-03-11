using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using HSR_SIM_CLIENT.ViewModels;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_CLIENT.Views;

public partial class BuffView : INotifyPropertyChanged
{
    public static readonly DependencyProperty BuffToViewProperty;

    static BuffView()
    {
        BuffToViewProperty = DependencyProperty.Register(nameof(BuffToView), typeof(Buff),
            typeof(BuffView), new FrameworkPropertyMetadata(PropChangedCb));
    }

    private static void PropChangedCb(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BuffView bv)
            bv.RefreshData();
    }

    public Buff BuffToView
    {
        get => (Buff)GetValue(BuffToViewProperty);
        set => SetValue(BuffToViewProperty, value);
    }

    private BuffViewModel? viewModel;

    public BuffViewModel ViewModel => viewModel ??= new BuffViewModel(BuffToView);

    public BuffView()
    {
        InitializeComponent();
    }

    private void RefreshData()
    {
        viewModel = null;
        NotifyPropertyChanged(nameof(ViewModel));
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
}