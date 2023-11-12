using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HSR_SIM_LIB.Utils;
using System.Windows.Forms;
using System.Xml;
using Tesseract;
using static HSR_SIM_GUI.GuiUtils;

namespace HSR_SIM_GUI.DamageTools
{
    internal class OcrUtils
    {
        public enum RectModeEnm
        {
            Minus,
            Plus

        }

        public record RStatWordRec
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public RectModeEnm StatMode { get; set; }

        }
        private bool isDown;
        private int initialX;
        private int initialY;
        private int finishX;
        private int finishY;
        private RectModeEnm rectMode;
        private Rectangle selectRect;

        Graphics formGraphics;



        private Rectangle LoadRectFromConfig(RectModeEnm rectModeEnm)
        {
            int.TryParse(IniF.IniReadValue("OcrUtils", rectModeEnm + "initialX"), out initialX);
            int.TryParse(IniF.IniReadValue("OcrUtils", rectModeEnm + "initialY"), out initialY);
            int.TryParse(IniF.IniReadValue("OcrUtils", rectModeEnm + "finishX"), out finishX);
            int.TryParse(IniF.IniReadValue("OcrUtils", rectModeEnm + "finishY"), out finishY);
            Rectangle res = new Rectangle(initialX, initialY, finishX - initialX, finishY - initialY);
            return res;
        }



        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isDown = false;
            finishX = e.X;
            finishY = e.Y;
            IniF.IniWriteValue("OcrUtils", rectMode + "finishX", finishX.ToString());
            IniF.IniWriteValue("OcrUtils", rectMode + "finishY", finishY.ToString());
            if (selectRect is { Height: > 0, Width: > 0 })
                ((Form)((Control)sender).Parent)?.Close();
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            isDown = true;
            initialX = e.X;
            initialY = e.Y;
            IniF.IniWriteValue("OcrUtils", rectMode + "initialX", initialX.ToString());
            IniF.IniWriteValue("OcrUtils", rectMode + "initialY", initialY.ToString());
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDown == true)
            {
                ((Control)sender).Refresh();
                Pen drwaPen = new Pen(rectMode == RectModeEnm.Minus ? Color.Red : Color.Green, 3);
                int width = e.X - initialX, height = e.Y - initialY;
                selectRect = new Rectangle(Math.Min(e.X, initialX),
                   Math.Min(e.Y, initialY),
                   Math.Abs(e.X - initialX),
                   Math.Abs(e.Y - initialY));
                formGraphics = ((Control)sender).CreateGraphics();
                formGraphics.DrawRectangle(drwaPen, selectRect);
            }
        }


        /// <summary>
        /// Get comparison items stats from  screen
        /// </summary>
        /// <param name="parentForm">form that call this func(will be focus set)</param>
        /// <param name="forceNewRect">ask for select new screen zones(false for use INI file saves) </param>
        /// <returns></returns>
        public Dictionary<int, RStatWordRec> GetComparisonItemStat(Form parentForm, bool forceNewRect)
        {
            Dictionary<int, RStatWordRec> res = new Dictionary<int, RStatWordRec>();
            nint hsrWindow = FindWindow(null, "Honkai: Star Rail");
            SetForegroundWindow(hsrWindow);
            WaitForActiveWindow(hsrWindow, 5);
            Bitmap hsrScreen = GraphicsCls.ConvertBlackAndWhite(CaptureScreen(false));
            string loc = "rus";
            //init engines LSTM for text, Legacy for numbers
            var engine = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory + "tessdata", loc, EngineMode.LstmOnly);
            var engineNumbers = new TesseractEngine(AppDomain.CurrentDomain.BaseDirectory + "tessdata", loc, EngineMode.TesseractOnly);
            engine.DefaultPageSegMode = PageSegMode.SingleBlock;
            engineNumbers.DefaultPageSegMode = PageSegMode.SingleBlock;


            foreach (RectModeEnm itemRectMode in (RectModeEnm[])Enum.GetValues(typeof(RectModeEnm)))
            {
                rectMode = itemRectMode;
                using (var form = new Form())
                {
                    selectRect = LoadRectFromConfig(itemRectMode);
                    if (selectRect.Height == 0 || selectRect.Width == 0 || forceNewRect)
                    {
                        form.Size = hsrScreen.Size;
                        PictureBox pictureBox1 = new PictureBox();

                        pictureBox1.Size = hsrScreen.Size;
                        pictureBox1.Image = (Bitmap)hsrScreen.Clone();
                        using Graphics gfx = Graphics.FromImage(pictureBox1.Image);
                        gfx.DrawString($"SELECT STAT NAMES({itemRectMode}) PARSE AREA(HOLD DOWN MOUSE AND MOVE)",
                            new("Tahoma", 13, FontStyle.Bold), new SolidBrush(itemRectMode == RectModeEnm.Minus ? Color.Red : Color.YellowGreen),
                            new PointF(hsrScreen.Width / 3, 150));
                        form.Controls.Add(pictureBox1);
                        form.FormBorderStyle = FormBorderStyle.None;
                        pictureBox1.MouseMove += PictureBox_MouseMove;
                        pictureBox1.MouseDown += PictureBox_MouseDown;
                        pictureBox1.MouseUp += PictureBox_MouseUp;
                        SetForegroundWindow(form.Handle);
                        form.ShowDialog();


                    }


                    Bitmap miniHsrScreen = hsrScreen.Clone(selectRect, hsrScreen.PixelFormat);
                    SetForegroundWindow(parentForm.Handle);

                    using (var img = PixConverter.ToPix(miniHsrScreen))
                    {
                        var page = engine.Process(img);
                        var pageNumbers = engineNumbers.Process(img);
                        var text = page.GetText();
                        var textNumbers = pageNumbers.GetText();
                        text = text.Replace("\n\n", "\n");
                        textNumbers = textNumbers.Replace("\n\n", "\n");
                        if (text.EndsWith("\n"))
                            text = text[..^1];
                        if (textNumbers.EndsWith("\n"))
                            textNumbers = textNumbers[..^1];
                        List<string> strings = text.Split('\n').ToList();
                        List<string> stringsNumbers = textNumbers.Split('\n').ToList();
                        //replace some shit into stat words
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\tessdata\\{loc}.xml");
                        XmlElement xRoot = xDoc.DocumentElement;
                        int toList = 0;

                        if (xRoot != null)
                        {
                            for (int i = 0; i < strings.Count; i++)
                            {
                                //parse all items
                                foreach (XmlElement xnode in xRoot)
                                {
                                    if (xnode.Name == "replacer")
                                    {
                                        string wordFrom = xnode.Attributes.GetNamedItem("from").Value.Trim();
                                        string wordTo = xnode.Attributes.GetNamedItem("to")?.Value.Trim();

                                        int wordNdx = strings[i].IndexOf(wordFrom);
                                        if (wordNdx >= 0)
                                        {
                                            Regex rx = new Regex("[0-9]");
                                            string val = "";
                                            int ndx = rx.Matches(stringsNumbers[i])
                                                .FirstOrDefault(x => x.Index > wordNdx)?.Index ?? 0;
                                            if (ndx > 0)
                                                val = stringsNumbers[i].Substring(ndx).Replace(" ", string.Empty);
                                            else
                                                val = strings[i].Substring(rx.Matches(strings[i]).First(x => x.Index > wordNdx).Index).Replace(" ", string.Empty);
                                            string key = wordTo + (val.EndsWith("%") ? "_prc" : "_fix");
                                            res.Add(res.Count, new RStatWordRec() { Key = key, Value = val, StatMode = itemRectMode });
                                            break;
                                        }



                                    }
                                }
                            }
                        }

                        page.Dispose();
                        pageNumbers.Dispose();

                    }

                }
            }

            forceNewRect = false;
            engine.Dispose();
            engineNumbers.Dispose();
            return res;
        }
    }
}
