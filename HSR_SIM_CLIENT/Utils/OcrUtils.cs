using System.Text.RegularExpressions;
using System.Xml;
using HSR_SIM_LIB.Utils;
using Tesseract;
using static HSR_SIM_CLIENT.Utils.GuiUtils;

namespace HSR_SIM_CLIENT.Utils;

public class OcrUtils
{
    /// <summary>
    ///     Utilities for text recognition
    /// </summary>
    public enum RectModeEnm
    {
        Minus,
        Plus
    }

    private int finishX;
    private int finishY;

    private Graphics formGraphics;
    private int initialX;
    private int initialY;
    private bool isDown;
    private RectModeEnm rectMode;
    private Rectangle selectRect;


    private Rectangle LoadRectFromConfig(RectModeEnm rectModeEnm)
    {
        int.TryParse(IniF.IniReadValue("OcrUtils", rectModeEnm + "initialX"), out initialX);
        int.TryParse(IniF.IniReadValue("OcrUtils", rectModeEnm + "initialY"), out initialY);
        int.TryParse(IniF.IniReadValue("OcrUtils", rectModeEnm + "finishX"), out finishX);
        int.TryParse(IniF.IniReadValue("OcrUtils", rectModeEnm + "finishY"), out finishY);
        var res = new Rectangle(initialX, initialY, finishX - initialX, finishY - initialY);
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

    private static void Negate(Bitmap image)
    {
        int x, y;

        // Loop through the images pixels to reset color.
        for (x = 0; x < image.Width; x++)
        for (y = 0; y < image.Height; y++)
        {
            var pixelColor = image.GetPixel(x, y);
            var newColor = Color.FromArgb(0xff - pixelColor.R
                , 0xff - pixelColor.G, 0xff - pixelColor.B);
            image.SetPixel(x, y, newColor);
        }
    }

    private void PictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDown)
        {
            ((Control)sender).Refresh();
            var drwaPen = new Pen(rectMode == RectModeEnm.Minus ? Color.Red : Color.Green, 3);
            int width = e.X - initialX, height = e.Y - initialY;
            selectRect = new Rectangle(Math.Min(e.X, initialX),
                Math.Min(e.Y, initialY),
                Math.Abs(e.X - initialX),
                Math.Abs(e.Y - initialY));
            formGraphics = ((Control)sender).CreateGraphics();
            formGraphics.DrawRectangle(drwaPen, selectRect);
        }
    }

    public static string GetTessDataFolder()
    {
        return AppDomain.CurrentDomain.BaseDirectory + "tessdata";
    }

    /// <summary>
    ///     Get comparison items stats from  screen
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="forceNewRect">ask for select new screen zones(false for use INI file saves) </param>
    /// <param name="loc">localization</param>
    /// <returns></returns>
    public Dictionary<int, RStatWordRec> GetComparisonItemStat(long hwnd, ref bool forceNewRect, string loc)
    {
        var res = new Dictionary<int, RStatWordRec>();
        var hsrWindow = FindWindow(null, "Honkai: Star Rail");
        SetForegroundWindow(hsrWindow);
        WaitForActiveWindow(hsrWindow, 5);
        var hsrScreen = CaptureScreen(false);
        //init engines LSTM for text, Legacy for numbers
        var engine = new TesseractEngine(GetTessDataFolder(), loc, EngineMode.LstmOnly);
        engine.DefaultPageSegMode = PageSegMode.SingleBlock;

        //need 2 picture rectangles. One is unequipped item, second is equipped item
        foreach (var itemRectMode in (RectModeEnm[])Enum.GetValues(typeof(RectModeEnm)))
        {
            rectMode = itemRectMode;
            using (var form = new Form())
            {
                selectRect = LoadRectFromConfig(itemRectMode);
                if (selectRect.Height == 0 || selectRect.Width == 0 || forceNewRect)
                {
                    form.Size = hsrScreen.Size;
                    var pictureBox1 = new PictureBox();

                    pictureBox1.Size = hsrScreen.Size;
                    pictureBox1.Image = (Bitmap)hsrScreen.Clone();
                    using var gfx = Graphics.FromImage(pictureBox1.Image);
                    gfx.DrawString($"SELECT STAT NAMES({itemRectMode}) PARSE AREA(HOLD DOWN MOUSE AND MOVE)",
                        new Font("Tahoma", 13, FontStyle.Bold),
                        new SolidBrush(itemRectMode == RectModeEnm.Minus ? Color.Red : Color.YellowGreen),
                        new PointF(hsrScreen.Width / 3, 150));
                    form.Controls.Add(pictureBox1);
                    form.FormBorderStyle = FormBorderStyle.None;
                    pictureBox1.MouseMove += PictureBox_MouseMove;
                    pictureBox1.MouseDown += PictureBox_MouseDown;
                    pictureBox1.MouseUp += PictureBox_MouseUp;
                    SetForegroundWindow(form.Handle);
                    form.ShowDialog();
                }


                //copy part of screenshot
                var miniHsrScreen = hsrScreen.Clone(selectRect, hsrScreen.PixelFormat);
                //remove green upgrade step count
                RemoveGreen(miniHsrScreen);
                //Orange to white
                RedToWhite(miniHsrScreen);
                // do it black and white
                miniHsrScreen = GraphicsCls.ConvertBlackAndWhite(miniHsrScreen);
                //negate it to make text black and background white
                Negate(miniHsrScreen);
                //GrayToWhite
                GreyToWhite(miniHsrScreen);

                SetForegroundWindow((IntPtr)hwnd);
                miniHsrScreen.Save($"test{itemRectMode}.bmp"); // uncoment to debug image processing
                using (var img = PixConverter.ToPix(miniHsrScreen))
                {
                    var page = engine.Process(img);
                    var text = page.GetText();

                    text = text.Replace("\n\n", "\n");
                    if (text.EndsWith("\n"))
                        text = text[..^1];
                    var strings = text.Split('\n').ToList();
                    //replace some shit into stat words
                    var xDoc = new XmlDocument();
                    xDoc.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\tessdata\\{loc}.xml");
                    var xRoot = xDoc.DocumentElement;

                    var replacers = new Dictionary<string, string>();

                    //parse replacers
                    foreach (XmlElement xnode in xRoot.SelectNodes("replacer"))
                        replacers.Add(xnode.Attributes.GetNamedItem("from").Value.Trim(),
                            xnode.Attributes.GetNamedItem("to")?.Value.Trim());

                    if (xRoot != null)
                        for (var i = 0; i < strings.Count; i++)
                        {
                            var replacerFound = false;
                            //parse all items
                            foreach (var replacer in replacers)
                            {
                                var wordFrom = replacer.Key;
                                var wordTo = replacer.Value;

                                var wordNdx = strings[i].IndexOf(wordFrom, StringComparison.Ordinal);
                                if (wordNdx >= 0)
                                {
                                    var rx = new Regex("[0-9]");
                                    var val = "";
                                    var ndx = rx.Matches(strings[i])
                                        .FirstOrDefault(x => x.Index > wordNdx)?.Index ?? 0;
                                    if (ndx > 0)
                                        val = strings[i].Substring(ndx).Replace(" ", string.Empty);
                                    else
                                        val = strings[i]
                                            .Substring(rx.Matches(strings[i]).FirstOrDefault(x => x.Index > wordNdx)
                                                ?.Index ?? strings[i].Length)
                                            .Replace(" ", string.Empty);
                                    var key = wordTo + (val.EndsWith("%") ? "_prc" : "_fix");

                                    replacerFound = true;
                                    res.Add(res.Count,
                                        new RStatWordRec { Key = key, Value = val, StatMode = itemRectMode });
                                    //key got replaced then exit foreach
                                    break;
                                }
                            }

                            //if item was not found in replacers
                            if (!replacerFound)
                                res.Add(res.Count,
                                    new RStatWordRec
                                        { Key = strings[i], Value = strings[i], StatMode = itemRectMode });
                        }

                    page.Dispose();
                }
            }
        }

        forceNewRect = false;
        engine.Dispose();
        return res;
    }

    /// <summary>
    ///     Remove green circles with upgrade steps
    /// </summary>
    /// <param name="image">Bitmap to process</param>
    /// <param name="threshold">green value over other colors to replace by black</param>
    private void RemoveGreen(Bitmap image, int threshold = 10)
    {
        int x, y;
        for (x = 0; x < image.Width; x++)
        for (y = 0; y < image.Height; y++)
        {
            var pixelColor = image.GetPixel(x, y);
            if (pixelColor.G - threshold > pixelColor.B && pixelColor.G - threshold > pixelColor.R)
            {
                var newColor = Color.FromArgb(0, 0, 0);
                image.SetPixel(x, y, newColor);
            }
        }
    }

    /// <summary>
    ///     Make orange text white(like other text)
    /// </summary>
    /// <param name="image">Bitmap to process</param>
    /// <param name="threshold">green value over other colors to replace by black</param>
    private void RedToWhite(Bitmap image, int threshold = 50)
    {
        int x, y;
        for (x = 0; x < image.Width; x++)
        for (y = 0; y < image.Height; y++)
        {
            var pixelColor = image.GetPixel(x, y);
            if (pixelColor.R - threshold > pixelColor.G && pixelColor.R - threshold > pixelColor.B)
            {
                var newColor = Color.FromArgb(255, 255, 255);
                image.SetPixel(x, y, newColor);
            }
        }
    }


    /// <summary>
    ///     Make grey text white(like background)
    /// </summary>
    /// <param name="image">Bitmap to process</param>
    /// <param name="threshold">green value over other colors to replace by black</param>
    /// <param name="thresholdTop">green value over other colors to replace by black(at top part of image)</param>
    private void GreyToWhite(Bitmap image, int threshold = 530, int thresholdTop = 440)
    {
        int x, y;
        for (x = 0; x < image.Width; x++)
        for (y = 0; y < image.Height; y++)
        {
            var pixelColor = image.GetPixel(x, y);
            if (pixelColor.R + pixelColor.G + pixelColor.B >= threshold ||
                (pixelColor.R + pixelColor.G + pixelColor.B >= thresholdTop && y <= image.Height / 4))
            {
                var newColor = Color.FromArgb(255, 255, 255);
                image.SetPixel(x, y, newColor);
            }
        }
    }

    public record RStatWordRec
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public RectModeEnm StatMode { get; set; }
    }
}