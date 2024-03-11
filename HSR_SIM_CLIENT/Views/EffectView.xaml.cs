using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HSR_SIM_CLIENT.ViewModels;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_CLIENT.Views;

public partial class EffectView : INotifyPropertyChanged
{
    public static readonly DependencyProperty EffectToViewProperty;

    static EffectView()
    {
        EffectToViewProperty = DependencyProperty.Register(nameof(EffectToView), typeof(Effect),
            typeof(EffectView), new FrameworkPropertyMetadata(PropChangedCb));
    }
    
    public EffectView()
    {
        InitializeComponent();
    }
    
    private static void PropChangedCb(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EffectView ev)
            ev.RefreshData();
    }

    public Effect EffectToView
    {
        get => (Effect)GetValue(EffectToViewProperty);
        set => SetValue(EffectToViewProperty, value);
    }

    private EffectViewModel? viewModel;

    public EffectViewModel ViewModel => viewModel ??= new EffectViewModel(EffectToView);

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