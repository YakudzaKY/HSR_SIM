using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using HSR_SIM_CLIENT.ViewModels;

namespace HSR_SIM_CLIENT.Views;

public partial class ConditionView : INotifyPropertyChanged
{
    public static readonly DependencyProperty ConditionToViewProperty;

    static ConditionView()
    {
        ConditionToViewProperty = DependencyProperty.Register(nameof(ConditionToView), typeof(ConditionViewModel),
            typeof(ConditionView), new FrameworkPropertyMetadata());
    }

    public ConditionView()
    {
        InitializeComponent();
    }


    public ConditionViewModel ConditionToView
    {
        get => (ConditionViewModel)GetValue(ConditionToViewProperty);
        set => SetValue(ConditionToViewProperty, value);
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