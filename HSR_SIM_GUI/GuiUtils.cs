using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Ini;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.Design;
using HSR_SIM_LIB.Utils;
using System.Drawing.Imaging;
using System.Threading;

namespace HSR_SIM_GUI
{
    internal static class GuiUtils
    {
        public static IniFile IniF = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "config.ini");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="box"></param>
        /// <param name="text"></param>
        /// <param name="maxLine"></param>
        public static void AddLine(this RichTextBox box, string text, uint? maxLine = null)
        {
            string newLineIndicator = "\n";

            /*max line check*/
            if (maxLine != null && maxLine > 0)
            {
                if (box.Lines.Count() >= maxLine)
                {
                    List<string> lines = box.Lines.ToList();
                    lines.RemoveAt(0);
                    box.Lines = lines.ToArray();
                }
            }

            /*add text*/
            string line = String.IsNullOrEmpty(box.Text) ? text : newLineIndicator + text;
            box.AppendText(line);
        }

        public static void ApplyDarkLightTheme(Chart chart)
        {
            [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
            static extern bool ShouldSystemUseDarkMode();

            if (ShouldSystemUseDarkMode())
            {
                chart.BackColor = GuiUtils.Zcolor(45, 45, 48);
                foreach (ChartArea chartArea in   chart.ChartAreas)
                {
                    chartArea.BackColor = GuiUtils.Zcolor(104, 104, 104);
                    chartArea.BorderColor =  Constant.clrDefault;
                    chartArea.AxisX.LabelStyle.ForeColor =Constant.clrDefault;
                    chartArea.AxisY.LabelStyle.ForeColor =Constant.clrDefault;
                    chartArea.AxisX.TitleForeColor =Constant.clrDefault;
                    chartArea.AxisY.TitleForeColor =Constant.clrDefault;
                    foreach (var axis in chartArea.Axes)
                    {
                        axis.LineColor = Constant.clrDefault;
                    }
                }
                foreach (Legend legend in   chart.Legends)
                {
                    legend.BackColor = GuiUtils.Zcolor(104, 104, 104);
                }
                
                foreach (var series in   chart.Series)
                {
                    series.LabelForeColor = Constant.clrDefault;
                }
             
                foreach (var title in   chart.Titles)
                {
                    title.ForeColor = Constant.clrDefault;
                }
            }

            
        }

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;

        
        /// <summary>
        /// get screenshot
        /// </summary>
        /// <param name="CaptureMouse"></param>
        /// <returns></returns>
        public static Bitmap CaptureScreen(bool CaptureMouse)
        {
            Bitmap result = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);

            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

                    if (CaptureMouse)
                    {
                        CURSORINFO pci;
                        pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                        if (GetCursorInfo(out pci))
                        {
                            if (pci.flags == CURSOR_SHOWING)
                            {
                                DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                                g.ReleaseHdc();
                            }
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
        static extern IntPtr GetForegroundWindow();
        public static int WaitForActiveWindow(nint windowHandle, int seconds)
        {
            DateTime startTime;
            string activeTitle = "";

            startTime = DateTime.Now;
            IntPtr activefForegroundWindow = GetForegroundWindow();

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

        public static void ApplyDarkLightTheme(Form form)
        {
            [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
            static extern bool ShouldSystemUseDarkMode();

            if (ShouldSystemUseDarkMode())
                ApplyTheme(form, GuiUtils.Zcolor(30, 30, 30), GuiUtils.Zcolor(45, 45, 48), GuiUtils.Zcolor(104, 104, 104), GuiUtils.Zcolor(51, 51, 51), Color.Black, Constant.clrDefault);

            else
                ApplyTheme(form, Color.White, GuiUtils.Zcolor(240, 240, 240), GuiUtils.Zcolor(181, 181, 181), GuiUtils.Zcolor(110, 110, 110), Color.White, Color.Black);
        }


        static void ApplyTheme(Form form, Color back, Color pan, Color btn, Color tbox, Color combox, Color textColor)
        {
            form.BackColor = back;

            foreach (Control item in form.Controls)
            {
                ApplyThemeToControl(item, back, pan, btn, tbox, combox, textColor);


            }


        }

        private static void ApplyThemeToControl(Control item, Color back, Color pan, Color btn, Color tbox, Color combox, Color textColor)
        {
            if (item is System.Windows.Forms.DataGridView)
            {
                ((DataGridView)item).BackColor = btn;
                ((DataGridView)item).ForeColor = textColor;
                ((DataGridView)item).GridColor = btn;
                ((DataGridView)item).BackgroundColor = btn;
                ((DataGridView)item).ColumnHeadersDefaultCellStyle.BackColor = combox;
                ((DataGridView)item).ColumnHeadersDefaultCellStyle.ForeColor = textColor;
                ((DataGridView)item).EnableHeadersVisualStyles = false;
                foreach ( var col in  ((DataGridView)item).Columns)
                {
                    ((DataGridViewTextBoxColumn)col).DefaultCellStyle.ForeColor = textColor;
                    ((DataGridViewTextBoxColumn)col).DefaultCellStyle.BackColor = tbox;
                }

            }
              
            else if (!(item is System.Windows.Forms.Label))
            {
                item.BackColor = btn;
                item.ForeColor = textColor;


                
            }
            else
            {
                item.ForeColor = textColor;
            }

            foreach (Control subitem in item.Controls)
            {
                ApplyThemeToControl(subitem, back, pan, btn, tbox, combox, textColor);
            }
        }

        public static Color Zcolor(int r, int g, int b)
        {
            return Color.FromArgb(r, g, b);
        }



    }

}
