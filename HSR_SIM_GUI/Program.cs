using System;
using System.Windows.Forms;

namespace HSR_SIM_GUI;

internal static class Program
{
    /// <summary>
    ///     Главная точка входа для приложения.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Main());
    }
}