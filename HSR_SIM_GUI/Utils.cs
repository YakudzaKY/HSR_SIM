using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ini;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.Design;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_GUI
{
    internal static class Utils
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

        public static void ApplyDarkLightTheme(Form form)
        {
            [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
            static extern bool ShouldSystemUseDarkMode();

            if (ShouldSystemUseDarkMode())
                ApplyTheme(form, Utils.Zcolor(30, 30, 30), Utils.Zcolor(45, 45, 48), Utils.Zcolor(104, 104, 104), Utils.Zcolor(51, 51, 51), Color.Black, Constant.clrDefault);

            else
                ApplyTheme(form, Color.White, Utils.Zcolor(240, 240, 240), Utils.Zcolor(181, 181, 181), Utils.Zcolor(110, 110, 110), Color.White, Color.Black);
        }
        static void ApplyTheme(Form form, Color back, Color pan, Color btn, Color tbox, Color combox, Color textColor)
        {
            form.BackColor = back;

            foreach (Control item in form.Controls)
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

            }


        }
        public static Color Zcolor(int r, int g, int b)
        {
            return Color.FromArgb(r, g, b);
        }



    }

}
