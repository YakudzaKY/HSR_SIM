using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using HSR_SIM_CLIENT.ViewModels;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CLIENT.Views;

public partial class BuffView : INotifyPropertyChanged
{
    public static readonly DependencyProperty BuffToViewProperty;
    

    static BuffView()
    {
        BuffToViewProperty = DependencyProperty.Register(nameof(BuffToView), typeof(BuffViewModel),
            typeof(BuffView), new FrameworkPropertyMetadata());
        
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