using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HSR_SIM_CLIENT.ViewModels;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CLIENT.Views;

public partial class EventView : INotifyPropertyChanged
{
    public static readonly DependencyProperty EventToViewProperty;
    private Formula? selectedFormula;

    static EventView()
    {
        EventToViewProperty = DependencyProperty.Register(nameof(EventToView), typeof(EventViewModel),
            typeof(EventView), new FrameworkPropertyMetadata(PropChangedCb));
    }


    public EventView()
    {        InitializeComponent();

       
    }


    public EventViewModel? EventToView
    {
        get => (EventViewModel)GetValue(EventToViewProperty);
        set => SetValue(EventToViewProperty, value);
    }

    public Visibility ExplainVisible => EventToView?.CalculateValue is Formula ? Visibility.Visible : Visibility.Hidden;

    public Formula? SelectedFormula
    {
        get => selectedFormula;
        set
        {
            if (Equals(value, selectedFormula)) return;
            selectedFormula = value;

            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static void PropChangedCb(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EventView ev)
            ev.RefreshData();
    }


    private void RefreshData()
    {
        
        SelectedFormula = null;
        NotifyPropertyChanged(nameof(ExplainVisible));
    }


    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }



    private void TreeView_OnSelectedFormulaChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // on event click
        if (((TreeView)sender).SelectedItem is Formula fm)

            SelectedFormula = fm;
    }
}