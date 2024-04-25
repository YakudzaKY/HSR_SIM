using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using HSR_SIM_CLIENT.ViewModels;

namespace HSR_SIM_CLIENT.Views;

public partial class EffectView : INotifyPropertyChanged
{
    public static readonly DependencyProperty EffectToViewProperty;


    static EffectView()
    {
        EffectToViewProperty = DependencyProperty.Register(nameof(EffectToView), typeof(EffectViewModel),
            typeof(EffectView), new FrameworkPropertyMetadata());
    }

    public EffectView()
    {
        InitializeComponent();
    }


    public EffectViewModel EffectToView
    {
        get => (EffectViewModel)GetValue(EffectToViewProperty);
        set => SetValue(EffectToViewProperty, value);
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