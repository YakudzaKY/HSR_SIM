using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Size = System.Drawing.Size;


namespace HSR_SIM_CLIENT.Utils;

/// <summary>
///     Class with utils for graphical interface and forms
/// </summary>
internal static class GuiUtils
{
    private const int CursorShowing = 0x00000001;
    public static readonly IniFile IniF = new(AppDomain.CurrentDomain.BaseDirectory + "config.ini");

    public static BitmapImage ToBitmapImage(this Bitmap bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Png);
        memory.Position = 0;

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        return bitmapImage;
    }

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool GetCursorInfo(out CursorInfo pci);

    [DllImport("user32.dll")]
    private static extern bool DrawIcon(IntPtr hDc, int x, int y, IntPtr hIcon);


    /// <summary>
    ///     get screenshot
    /// </summary>
    /// <param name="captureMouse"></param>
    /// <returns></returns>
    public static Bitmap CaptureScreen(bool captureMouse)
    {
       
        var result = new Bitmap((int)  SystemParameters.PrimaryScreenWidth,(int) SystemParameters.PrimaryScreenHeight, PixelFormat.Format24bppRgb);


        using var g = Graphics.FromImage(result);
        g.CopyFromScreen(0, 0, 0, 0, new Size(){Width = (int) SystemParameters.FullPrimaryScreenWidth,Height = (int)SystemParameters.FullPrimaryScreenHeight}, CopyPixelOperation.SourceCopy);

        if (captureMouse)
        {
            CursorInfo pci;
            pci.cbSize = Marshal.SizeOf(typeof(CursorInfo));

            if (GetCursorInfo(out pci))
                if (pci.flags == CursorShowing)
                {
                    DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                    g.ReleaseHdc();
                }
        }


        return result;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    public static int WaitForActiveWindow(nint windowHandle, int seconds)
    {
        var startTime = DateTime.Now;
        var activeForegroundWindow = GetForegroundWindow();

        while (windowHandle != activeForegroundWindow)
        {
            //Check for timeout
            if ((DateTime.Now - startTime).TotalSeconds > seconds) //greater than here
                return 1;

            //Check every 0.2 seconds
            Thread.Sleep(200);
            activeForegroundWindow = GetForegroundWindow();
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
    private struct CursorInfo
    {
        public Int32 cbSize;
        public Int32 flags;
        public IntPtr hCursor;
        public PointApi ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PointApi
    {
        public int x;
        public int y;
    }
}