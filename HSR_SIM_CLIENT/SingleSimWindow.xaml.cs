using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using HSR_SIM_LIB;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CLIENT;

public partial class SingleSimWindow : Window
{
    private Worker wrk;
    private bool busy = false;
    public bool DbgMode { get; set; } = false;

    public SingleSimWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     For text callback
    /// </summary>
    /// <param name="kv"></param>
    public void WorkerCallBackString(KeyValuePair<string, string> kv)
    {
        if (string.Equals(kv.Key, Constant.MsgLog))
        {
            LogTextBlock.AppendText(kv.Value);
            if (!busy)
                LogTextBlock.ScrollToEnd();
        }
        else if (string.Equals(kv.Key, Constant.MsgDebug))
        {
            if (DbgMode)
            {
                DebugTextBlock.AppendText(kv.Value);
                DebugTextBlock.AppendText(Environment.NewLine);
                if (!busy)
                    DebugTextBlock.ScrollToEnd();
            }
        }
    }

    /// <summary>
    /// ask user what decision are pick from options(do we crit etc...)
    /// </summary>
    /// <param name="items"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public int WorkerCallBackGetDecision(string[] items, string description)
    {
        GetDecision getDecision = new GetDecision(items,description);
        getDecision.ShowDialog();
        return getDecision.ItemIndex;
    }

    /// <summary>
    /// Set worker fields
    /// </summary>
    /// <param name="simCls"></param>
    /// <param name="devModePath"></param>
    /// <param name="chkDevMode"></param>
    public void SetSim(SimCls simCls, string? devModePath = null, bool chkDevMode = false)
    {
        wrk = new Worker();
        wrk.CbLog += WorkerCallBackString;
        wrk.CbRend += WorkerCallBackImages;
        wrk.CbGetDecision = WorkerCallBackGetDecision;
        wrk.DevMode = chkDevMode;
        wrk.LoadScenarioFromSim(simCls, devModePath);
    }

    BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
        using (MemoryStream memory = new MemoryStream())
        {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();

            return bitmapimage;
        }
    }

    /// <summary>
    ///     For imagesCallback
    /// </summary>
    /// <param name="kv"></param>
    public void WorkerCallBackImages(Bitmap combatImg)
    {
        if (combatImg != null)
        {
            CombatImg.Source = BitmapToImageSource(combatImg);
        }
    }

    private void BtnNext_OnClick(object sender, RoutedEventArgs e)
    {
        wrk?.MoveStep();
    }
}