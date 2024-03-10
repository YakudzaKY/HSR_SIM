﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HSR_SIM_CLIENT.ViewModels;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CLIENT.Views;

public partial class EventView : INotifyPropertyChanged
{
    public static readonly DependencyProperty EventToViewProperty;

    static EventView()
    {
        EventToViewProperty = DependencyProperty.Register(nameof(EventToView), typeof(EventViewModel), typeof(EventView), new FrameworkPropertyMetadata( propertyChangedCallback:new PropertyChangedCallback(PropChangedCb)));
    }

    private static void PropChangedCb(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {

        if (d is EventView ev)
            ev.RefreshData();
    }
   



    public EventView()
    {
        InitializeComponent();
    }


    private void RefreshData()
    {
        NotifyPropertyChanged(nameof(ExplainVisible));
    }
        



    public  EventViewModel EventToView
    {
       get => (EventViewModel)GetValue(EventToViewProperty);
       set =>SetValue(EventToViewProperty, value) ;
       
       
    }

    public Visibility ExplainVisible => (EventToView?.CalculateValue is Formula) ? Visibility.Visible : Visibility.Hidden;
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
    
    private void downButton_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void upButton_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}