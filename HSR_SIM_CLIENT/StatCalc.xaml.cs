using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace HSR_SIM_CLIENT;

public partial class StatCalc : Window
{
    public StatCalc()
    {
        InitializeComponent();
    }
    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}