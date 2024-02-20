using System.Windows;

namespace HSR_SIM_CLIENT;

public partial class GetDecision : Window
{
    public string [] Items { get;  }
    public string Description { get; }
    public int ItemIndex { get; set; }

    public GetDecision(string [] items,string description)
    {
        ItemIndex = 0;
        
        Items = items;
        Description = description;
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}