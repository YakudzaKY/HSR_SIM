using System.Windows;

namespace HSR_SIM_CLIENT;

public partial class GetDecision : Window
{
    public string Items { get;  }
    public string ItemIndex { get; set; }

    public GetDecision(string items)
    {
        Items = items;
        InitializeComponent();
    }
}