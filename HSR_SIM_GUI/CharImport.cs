using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using HSR_SIM_LIB.Utils;
using Newtonsoft.Json;
using static HSR_SIM_GUI.GuiUtils;

namespace HSR_SIM_GUI;
/// <summary>
/// Character import from hoyo api.
/// </summary>
public partial class CharImport : Form
{
    private Character mainCharacter;

    public CharImport()
    {
        InitializeComponent();
        ApplyDarkLightTheme(this);
    }


    private static Form ShowLabelDialog(string input, string caption)
    {
        Size size = new(200, 70);
        Form inputBox = new()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ClientSize = size,
            Text = caption,
            StartPosition = FormStartPosition.CenterScreen
        };

        Label textBox = new()
        {
            Size = new Size(size.Width - 10, 23),
            Location = new Point(5, 5),
            Text = input
        };
        inputBox.Controls.Add(textBox);


        return inputBox;
    }


    private static DialogResult ShowInputDialog(ref string input, string caption)
    {
        Size size = new(200, 70);
        Form inputBox = new()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ClientSize = size,
            Text = caption,
            StartPosition = FormStartPosition.CenterScreen
        };

        TextBox textBox = new()
        {
            Size = new Size(size.Width - 10, 23),
            Location = new Point(5, 5),
            Text = input
        };
        inputBox.Controls.Add(textBox);

        Button okButton = new()
        {
            DialogResult = DialogResult.OK,
            Name = "okButton",
            Size = new Size(75, 23),
            Text = "&OK",
            Location = new Point(size.Width - 80 - 80, 39)
        };
        inputBox.Controls.Add(okButton);

        Button cancelButton = new()
        {
            DialogResult = DialogResult.Cancel,
            Name = "cancelButton",
            Size = new Size(75, 23),
            Text = "&Cancel",
            Location = new Point(size.Width - 80, 39)
        };
        inputBox.Controls.Add(cancelButton);

        inputBox.AcceptButton = okButton;
        inputBox.CancelButton = cancelButton;

        var result = inputBox.ShowDialog();
        input = textBox.Text;
        return result;
    }

    private static DialogResult ShowCbDialogResult(ref int index, string caption, List<Character> characters)
    {
        Size size = new(200, 70);
        Form inputBox = new()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ClientSize = size,
            Text = caption,
            StartPosition = FormStartPosition.CenterScreen
        };

        ComboBox cbBox = new()
        {
            Size = new Size(size.Width - 10, 23),
            Location = new Point(5, 5)
        };
        foreach (var character in characters) cbBox.Items.Add(character.Name);

        cbBox.SelectedIndex = 0;
        inputBox.Controls.Add(cbBox);

        Button okButton = new()
        {
            DialogResult = DialogResult.OK,
            Name = "okButton",
            Size = new Size(75, 23),
            Text = "&OK",
            Location = new Point(size.Width - 80 - 80, 39)
        };
        inputBox.Controls.Add(okButton);

        Button cancelButton = new()
        {
            DialogResult = DialogResult.Cancel,
            Name = "cancelButton",
            Size = new Size(75, 23),
            Text = "&Cancel",
            Location = new Point(size.Width - 80, 39)
        };
        inputBox.Controls.Add(cancelButton);

        inputBox.AcceptButton = okButton;
        inputBox.CancelButton = cancelButton;

        var result = inputBox.ShowDialog();
        index = cbBox.SelectedIndex;
        return result;
    }


    private static void FillAttributes(DataGridView view, List<Attribute> attributes)
    {
        view.Rows.Clear();
        if (attributes is null)
            return;
        if (attributes.Count <= 0)
            return;


        view.Rows.Add(attributes.Count);
        foreach (var attr in attributes)
        {
            view.Rows[attributes.IndexOf(attr)].Cells[0].Value = attr.field;
            view.Rows[attributes.IndexOf(attr)].Cells[1].Value = attr.value.ToString();
        }
    }

    /// <summary>
    ///     Load character from object
    /// </summary>
    /// <param name="character"></param>
    private void LoadCharacter()
    {
        txtLvl.Value = mainCharacter.Level;
        txtRank.Value = mainCharacter.Rank;
        AvatarBox.Image = new Bitmap(Utl.LoadBitmap("Character\\" + mainCharacter.Name),
            new Size(AvatarBox.Width, AvatarBox.Height));
        label1.Text = mainCharacter.Name;
        txtLC.Text = mainCharacter.light_cone.name;
        txtLCRank.Value = mainCharacter.light_cone.rank;
        txtLcLevel.Value = mainCharacter.light_cone.level;

        FillAttributes(dgStats, mainCharacter.attributes);

        dgSets.Rows.Clear();
        if (mainCharacter.relic_sets is { Count: > 0 })
        {
            dgSets.Rows.Add(mainCharacter.relic_sets.Count);
            foreach (var ger in mainCharacter.relic_sets)
            {
                dgSets.Rows[mainCharacter.relic_sets.IndexOf(ger)].Cells[0].Value = ger.name;
                dgSets.Rows[mainCharacter.relic_sets.IndexOf(ger)].Cells[1].Value = ger.num.ToString();
            }
        }


        //skills
        dgSkills.Rows.Clear();
        if (mainCharacter.skills.Count > 0)
            dgSkills.Rows.Add(mainCharacter.skills.Count);
        foreach (var skl in mainCharacter.skills)
        {
            dgSkills.Rows[mainCharacter.skills.IndexOf(skl)].Cells[0].Value = skl.name;
            dgSkills.Rows[mainCharacter.skills.IndexOf(skl)].Cells[1].Value = skl.level.ToString();
            dgSkills.Rows[mainCharacter.skills.IndexOf(skl)].Cells[2].Value = skl.max_level.ToString();
        }
    }


    private static void ConcatAndRenameAttrib(List<Attribute> lst_out, List<Attribute> lst_in)
    {
        foreach (var a in lst_in) a.field += a.percent ? "_prc" : "_fix";

        if (lst_in == lst_out)
            return;
        foreach (var a in lst_in)
            if (lst_out.Any(x => x.field == a.field))
                lst_out.First(x => x.field == a.field).value += a.value;
            else
                lst_out.Add(new Attribute { field = a.field, percent = a.percent, value = a.value });
    }

    /// <summary>
    ///     import from json API
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Import_Click(object sender, EventArgs e)
    {
        var input = IniF.IniReadValue("WarGear", "UID");
        var dRes = ShowInputDialog(ref input, "UID?");

        if (dRes == DialogResult.OK)
        {
            IniF.IniWriteValue("WarGear", "UID", input);
            //get from api
            var waitDialog = ShowLabelDialog("loading data...", "please wait");
            waitDialog.Show();
            using HttpClient wc = new();
            var result = await wc.GetStringAsync(string.Format("https://api.mihomo.me/sr_info_parsed/{0:s}?lang={1:s}",
                input, "en"));


            var data = JsonConvert.DeserializeObject<ApiData>(result);

            //add _prc to attribute names and concatinate all attributes
            foreach (var character in data.characters)
            {
                //concatinate and renaming stats
                ConcatAndRenameAttrib(character.attributes, character.properties);
                //delete low num sets
                List<GearSet> filtredSet = new();
                foreach (var set in character.relic_sets)
                    if (filtredSet.All(x => x.name != set.name))
                    {
                        filtredSet.Add(set);
                    }
                    else
                    {
                        var fndgs = filtredSet.First(x => x.name == set.name);
                        fndgs.num = Math.Max(fndgs.num, set.num);
                    }

                character.relic_sets = filtredSet;
            }

            var index = 0;
            waitDialog.Close();
            waitDialog.Dispose();
            //if not autosave
            if (!chAutoSave.Checked)
            {
                if (ShowCbDialogResult(ref index, "Choose character", data.characters) == DialogResult.OK)
                {
                    mainCharacter = data.characters[index];
                    LoadCharacter();
                }
            }
            else
            {
                //save all by default names
                foreach (var character in data.characters)
                    XmlSave(character, GetWarGearPath() + GetDefaultFileName(character));

                XElement profile = new("Profile");
                XElement party = new("Party");
                profile.Add(party);

                foreach (var character in data.characters)
                {
                    XElement unit = new("Unit");
                    unit.SetAttributeValue("template", "Character\\" + character.Name);
                    unit.SetAttributeValue("wargear", GetDefaultFileName(character, false));
                    party.Add(unit);
                }

                profile.Save(GetProfilePath() + string.Format("UID_{0:s}.xml", IniF.IniReadValue("WarGear", "UID")));
                // Code to write the stream goes here.
            }
        }
    }

    private void BindingSource1_CurrentChanged(object sender, EventArgs e)
    {
    }

    private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
    }

    private void DgStats_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
        var val = ((DataGridView)sender).Rows[e.RowIndex].Cells[1].Value?.ToString();
        var prop = ((DataGridView)sender).Rows[e.RowIndex].Cells[0].Value?.ToString();

        if (prop != null)
        {
            var charVal = mainCharacter.GetType().GetProperty(prop)?.GetValue(mainCharacter, null);
            if (charVal is int)
                mainCharacter.GetType().GetProperty(prop)?.SetValue(mainCharacter, int.Parse(val));
            else if (charVal is double)
                mainCharacter.GetType().GetProperty(prop)?.SetValue(mainCharacter, double.Parse(val));
            else
                mainCharacter.GetType().GetProperty(prop)?.SetValue(mainCharacter, val);
        }
    }

    private void DgStats_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
    }

    private void TxtLvl_ValueChanged(object sender, EventArgs e)
    {
        mainCharacter.Level = (int)((NumericUpDown)sender).Value;
    }

    private void TxtRank_ValueChanged(object sender, EventArgs e)
    {
        mainCharacter.Rank = (int)((NumericUpDown)sender).Value;
    }


    private static string GetWarGearPath()
    {
        return Utl.DataFolder + "\\WarGear\\";
    }

    private static string GetProfilePath()
    {
        return Utl.DataFolder + "\\Profile\\";
    }

    private static string GetDefaultFileName(Character character, bool withExt = true)
    {
        var ext = withExt ? ".xml" : "";
        return string.Format("{0:s}_{1:s}" + ext, character?.Name,
            IniF.IniReadValue("WarGear", "UID"));
    }

    private void SaveXML_Click(object sender, EventArgs e)
    {
        SaveFileDialog saveFileDialog1 = new()
        {
            Filter = "xml file (*.xml)|*.xml",
            FilterIndex = 1,
            InitialDirectory = Path.GetFullPath(GetWarGearPath()),
            RestoreDirectory = true,
            FileName = GetDefaultFileName(mainCharacter)
        };
        if (saveFileDialog1.ShowDialog() == DialogResult.OK) XmlSave(mainCharacter, saveFileDialog1.FileName);
    }

    /// <summary>
    /// save character gear and stats into XML
    /// </summary>
    /// <param name="character"></param>
    /// <param name="savePath"></param>
    private static void XmlSave(Character character, string savePath)
    {
        XElement unit = new("Unit");
        unit.SetAttributeValue("name", character?.Name);
        unit.SetAttributeValue("level", character.Level.ToString());
        unit.SetAttributeValue("rank", character.Rank.ToString());
        XElement stat = new("Stats");
        foreach (var attr in character.attributes) stat.SetAttributeValue(attr.field, attr.value);
        unit.Add(stat);

        if (character.light_cone != null)
        {
            XElement xLc = new("LightCone");
            xLc.SetAttributeValue("rank", character.light_cone.rank.ToString());
            xLc.SetAttributeValue("level", character.light_cone.level.ToString());
            xLc.SetAttributeValue("name", character.light_cone.name);
            unit.Add(xLc);
        }

        foreach (var skl in character.skills)
        {
            XElement skill = new("Skill");
            skill.SetAttributeValue("name", skl.name);
            skill.SetAttributeValue("level", skl.level.ToString());
            skill.SetAttributeValue("max_level", skl.max_level.ToString());
            unit.Add(skill);
        }


        foreach (var gearSet in character.relic_sets)
        {
            XElement set = new("RelicSet");
            set.SetAttributeValue("name", gearSet.name);
            set.SetAttributeValue("num", gearSet.num.ToString());
            unit.Add(set);
        }


        unit.Save(savePath);
    }

    private void Button1_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFile
            = new()
            {
                Filter = @"xml file (*.xml)|*.xml",
                FilterIndex = 1,
                InitialDirectory = Path.GetFullPath(GetWarGearPath()),
                RestoreDirectory = true
            };

        if (openFile.ShowDialog() == DialogResult.OK)
        {
            mainCharacter = XmlLoad(openFile.FileName);


            LoadCharacter();
        }
    }

    /// <summary>
    /// load character from xml
    /// </summary>
    /// <param name="openFileFileName"></param>
    /// <returns></returns>
    private static Character XmlLoad(string openFileFileName)
    {
        Character character
            = new();
        //Scenario
        XmlDocument xDoc = new();
        xDoc.Load(openFileFileName);
        var xRoot = xDoc.DocumentElement;
        if (xRoot != null)
        {
            character.Level = int.Parse(xRoot.Attributes.GetNamedItem("level")?.Value ?? "0");
            character.Name = xRoot.Attributes.GetNamedItem("name")?.Value;
            character.Rank = int.Parse(xRoot.Attributes.GetNamedItem("rank")?.Value ?? "0");
            character.attributes = new List<Attribute>();
            character.skills = new List<Skill>();
            character.relic_sets = new List<GearSet>();
            //parse all items
            foreach (XmlElement xnode in xRoot)
            {
                if (xnode.Name == "LightCone")
                    character.light_cone = new LCone
                    {
                        level = int.Parse(xnode.Attributes.GetNamedItem("level")?.Value ?? "0"),
                        name = xnode.Attributes.GetNamedItem("name")?.Value,
                        rank = int.Parse(xnode.Attributes.GetNamedItem("rank")?.Value ?? "0")
                    };

                if (xnode.Name == "Stats")
                    foreach (XmlAttribute xmlattr in xnode.Attributes)
                    {
                        Attribute attr = new()
                        {
                            field = xmlattr.Name,
                            value = double.Parse(xmlattr.Value.Replace(".", ","))
                        };
                        character.attributes.Add(attr);
                    }

                if (xnode.Name == "Skill")
                {
                    Skill skl = new()
                    {
                        name = xnode.Attributes.GetNamedItem("name")?.Value
                    };
                    ;
                    skl.level = int.Parse(xnode.Attributes.GetNamedItem("level")?.Value ?? "0");
                    skl.max_level = int.Parse(xnode.Attributes.GetNamedItem("max_level")?.Value ?? "0");
                    character.skills.Add(skl);
                }

                if (xnode.Name == "RelicSet")
                {
                    GearSet gs = new()
                    {
                        name = xnode.Attributes.GetNamedItem("name")?.Value,
                        num = int.Parse(xnode.Attributes.GetNamedItem("num")?.Value ?? "0")
                    };
                    character.relic_sets.Add(gs);
                }
            }
        }

        return character;
    }

    /// <summary>
    /// update character skill values after grid end edit
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DgSkills_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
        var rw = e.RowIndex;
        var skillName = ((DataGridView)sender).Rows[e.RowIndex].Cells[0].Value?.ToString();
        var lvl = int.Parse(((DataGridView)sender).Rows[e.RowIndex].Cells[1].Value.ToString());
        var max_lvl = int.Parse(((DataGridView)sender).Rows[e.RowIndex].Cells[2].Value.ToString());
        if (skillName != null)
        {
            mainCharacter.skills.First(x => x.name == skillName).level = lvl;
            mainCharacter.skills.First(x => x.name == skillName).max_level = max_lvl;
        }
    }

    private void Label8_Click(object sender, EventArgs e)
    {
    }

    private void TxtLCRank_ValueChanged(object sender, EventArgs e)
    {
        mainCharacter.light_cone.rank = (int)((NumericUpDown)sender).Value;
    }

    private void TxtLcLevel_ValueChanged(object sender, EventArgs e)
    {
        mainCharacter.light_cone.level = (int)((NumericUpDown)sender).Value;
    }

    private void DgSets_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
        var rw = e.RowIndex;
        var setName = ((DataGridView)sender).Rows[e.RowIndex].Cells[0].Value?.ToString();
        var num = int.Parse(((DataGridView)sender).Rows[e.RowIndex].Cells[1].Value.ToString());
        if (setName != null) mainCharacter.relic_sets.First(x => x.name == setName).num = num;
    }


    public class Player
    {
        public int Level;
        public string Nickname;
        public string Uid;
    }

    public class Attribute
    {
        public string field;
        public bool percent;
        public double value;
    }

    public class Skill
    {
        public int level;
        public int max_level;
        public string name;
    }

    public class Gear
    {
        public Attribute main_affix;
        public string name;
        public List<Attribute> sub_affix;
    }

    public class LCone
    {
        public List<Attribute> attributes;
        public int level;
        public string name;
        public int promotion;
        public List<Attribute> properties;
        public int rank;
    }

    public class GearSet
    {
        public string name;
        public int num;
        public List<Attribute> properties;
    }

    public class Character
    {
        public List<Attribute> attributes;

        public LCone light_cone;
        public List<Attribute> properties;
        public List<GearSet> relic_sets;
        public List<Gear> relics;

        public List<Skill> skills;
        public int ID { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Rank { get; set; }

        public int Promotion { get; set; }
    }

    public class ApiData
    {
        public List<Character> characters;
        public Player player;
    }
}