using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using HSR_SIM_LIB;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_GUI.GuiUtils;
using static HSR_SIM_LIB.Utils.CallBacks;
using static HSR_SIM_LIB.Worker;

namespace HSR_SIM_GUI;

public partial class GUIForm : Form
{
    private Worker wrk;

    private bool busy;
    private DebugWindow dbg;


    public GUIForm()
    {

        InitializeComponent();
        ApplyDarkLightTheme(this);
    }

    /// <summary>
    ///     For text callback
    /// </summary>
    /// <param name="kv"></param>
    public void WorkerCallBackString(KeyValuePair<string, string> kv)
    {
        if (string.Equals(kv.Key, Constant.MsgLog))
        {
            LogWindow.AddLine(kv.Value, 400);
            if (!busy)
                LogWindow.ScrollToCaret();
        }
        else if (string.Equals(kv.Key, Constant.MsgDebug))
        {
            if (dbg != null && !dbg.IsDisposed)
            {
                dbg.dbgText.AddLine(kv.Value);
                if (!busy)
                    LogWindow.ScrollToCaret();
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
        int yCur = 0;
        Size size = new(200, 180);
        Form inputBox = new()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ClientSize = size,
            Text = "Chose:",
            StartPosition = FormStartPosition.CenterScreen
        };
        yCur += 5;
        TextBox lvlDescr = new TextBox()
        {
            Size = new Size(size.Width - 10, 105),
            Text = description,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            Location = new Point(5, yCur)

        };
        inputBox.Controls.Add(lvlDescr);

        yCur += 110;
        ComboBox cbBox = new()
        {
            Size = new Size(size.Width - 10, 23),
            Location = new Point(5, yCur)
        };
        foreach (var vaItem in items)
        {
            cbBox.Items.Add(vaItem);
        }

        cbBox.SelectedIndex = 0;

        inputBox.Controls.Add(cbBox);
        yCur += 34;
        Button okButton = new()
        {
            DialogResult = DialogResult.OK,
            Name = "okButton",
            Size = new Size(75, 23),
            Text = "&OK",
            Location = new Point(size.Width - 80 - 80, yCur)
        };
        inputBox.Controls.Add(okButton);
        inputBox.AcceptButton = okButton;
        var result = inputBox.ShowDialog();
        return cbBox.SelectedIndex;

    }


    /// <summary>
    ///     For imagesCallback
    /// </summary>
    /// <param name="kv"></param>
    public void WorkerCallBackImages(Bitmap combatImg)
    {
        if (combatImg != null)
        {
            combatOut.BackgroundImage?.Dispose();
            combatOut.BackgroundImage = new Bitmap(combatImg);
        }
    }


    private void Main_Load(object sender, EventArgs e)
    {
        RefreshCbs();
        cbScenario.Text = IniF.IniReadValue("form", "Scenario");
        cbProfile.Text = IniF.IniReadValue("form", "Profile");
        string boolVal = IniF.IniReadValue("form", "DevMode");

        chkDevMode.Checked = !String.IsNullOrEmpty(boolVal) && bool.Parse(boolVal);

    }

    private void ControlsDispose()
    {
        IniF.IniWriteValue("form", "Scenario", cbScenario.Text);
        IniF.IniWriteValue("form", "Profile", cbProfile.Text);
        IniF.IniWriteValue("form", "DevMode", chkDevMode.Checked.ToString());
    }

    /// <summary>
    ///     Load scenario click
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button1_Click(object sender, EventArgs e)
    {
        wrk?.DevModeLog?.WriteToFile();//save prev data

        wrk = new Worker();
        wrk.CbLog += WorkerCallBackString;
        wrk.CbRend += WorkerCallBackImages;
        wrk.CbGetDecision = WorkerCallBackGetDecision;
        wrk.DevMode = chkDevMode.Checked;
        wrk.LoadScenarioFromXml(GetScenarioPath() + cbScenario.Text,
            String.IsNullOrEmpty(cbProfile.Text)?"":GetProfilePath() + cbProfile.Text);
    }


    private void RefreshCbs()
    {
        cbScenario.Items.Clear();
        cbProfile.Items.Clear();
        var files = Directory.GetFiles(GetScenarioPath(),"*.xml");

        foreach (var file in files) cbScenario.Items.Add(Path.GetFileName(file));

        Directory.CreateDirectory(GetProfilePath());
        files = Directory.GetFiles(GetProfilePath(),"*.xml");

        foreach (var file in files) cbProfile.Items.Add(Path.GetFileName(file));
    }

    private void Button3_Click(object sender, EventArgs e)
    {
        wrk?.MoveStep();
    }

    private void Button2_Click(object sender, EventArgs e)
    {
        wrk?.MoveStep(true);
    }

    private void Label1_Click(object sender, EventArgs e)
    {
    }

    private void Button1_Click_1(object sender, EventArgs e)
    {
        RefreshCbs();
    }

    private void Button2_Click_1(object sender, EventArgs e)
    {
        if (wrk is null) return;
        busy = true;
        wrk.CbRend -= WorkerCallBackImages;
        wrk.MoveStep(false, 100);
        wrk.CbRend += WorkerCallBackImages;
        busy = false;
        LogWindow.ScrollToCaret();
        wrk.DrawCombat();
    }

    private void Button3_Click_1(object sender, EventArgs e)
    {
        if (wrk is null) return;
        busy = true;
        wrk.CbRend -= WorkerCallBackImages;
        wrk.MoveStep(true, -1);
        wrk.CbRend += WorkerCallBackImages;
        busy = false;
        LogWindow.ScrollToCaret();
        wrk.DrawCombat();
    }

    private void Button4_Click(object sender, EventArgs e)
    {

    }

    private void Button5_Click(object sender, EventArgs e)
    {
        if (dbg == null || dbg.IsDisposed)
            dbg = new DebugWindow();
        dbg.Show();
    }

    private void button6_Click(object sender, EventArgs e)
    {
        if (wrk is null) return;
        wrk.Completed = false;
        wrk.MoveStep(false, 1, true);
    }

    private void BtnStatCheck_Click(object sender, EventArgs e)
    {
        StatCheck st = new()
        {
            StartPosition = FormStartPosition.CenterScreen
        };
        st.Show();
    }

    private void cbProfile_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    private void Main_FormClosed(object sender, FormClosedEventArgs e)
    {
        wrk?.DevModeLog?.WriteToFile();
    }

    private void btnClearDevMode_Click(object sender, EventArgs e)
    {
        //delete dev log
        File.Delete(DevModeUtils.GetDevLogPath(cbScenario.Text, cbProfile.Text));
    }

    private void button4_Click_1(object sender, EventArgs e)
    {
      
        wrk?.DevModeLog?.WriteResToFile();
    }
}