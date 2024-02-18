using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Ini;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace HSR_SIM_CLIENT;

/// <summary>
/// Class with utils for graphical interface and forms
/// </summary>
internal static class GuiUtils
{

    private const int CURSOR_SHOWING = 0x00000001;
    public static IniFile IniF = new(AppDomain.CurrentDomain.BaseDirectory + "config.ini");



    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool GetCursorInfo(out CURSORINFO pci);

    [DllImport("user32.dll")]
    private static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);


    /// <summary>
    ///     get screenshot
    /// </summary>
    /// <param name="CaptureMouse"></param>
    /// <returns></returns>
    public static Bitmap CaptureScreen(bool CaptureMouse)
    {
        var result = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height,
            PixelFormat.Format24bppRgb);

        try
        {
            using (var g = Graphics.FromImage(result))
            {
                g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

                if (CaptureMouse)
                {
                    CURSORINFO pci;
                    pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                    if (GetCursorInfo(out pci))
                        if (pci.flags == CURSOR_SHOWING)
                        {
                            DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                            g.ReleaseHdc();
                        }
                }
            }
        }
        catch
        {
            result = null;
        }

        return result;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    public static int WaitForActiveWindow(nint windowHandle, int seconds)
    {
        DateTime startTime;
  

        startTime = DateTime.Now;
        var activefForegroundWindow = GetForegroundWindow();

        while (windowHandle != activefForegroundWindow)
        {
            //Check for timeout
            if ((DateTime.Now - startTime).TotalSeconds > seconds) //greater than here
                return 1;

            //Check every 0.2 seconds
            Thread.Sleep(200);
            activefForegroundWindow = GetForegroundWindow();
        }

        return 0;
    }

    public static string GetScenarioPath()
    {
        return AppDomain.CurrentDomain.BaseDirectory + "DATA\\Scenario\\";
    }

    public static string GetProfilePath()
    {
        return AppDomain.CurrentDomain.BaseDirectory + "DATA\\Profile\\";
    }



    public static Color Zcolor(int r, int g, int b)
    {
        return Color.FromArgb(r, g, b);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CURSORINFO
    {
        public Int32 cbSize;
        public Int32 flags;
        public IntPtr hCursor;
        public POINTAPI ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINTAPI
    {
        public int x;
        public int y;
    }
}