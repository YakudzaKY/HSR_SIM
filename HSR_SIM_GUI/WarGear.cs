using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;
using static HSR_SIM_GUI.Utils;
using System.Text.Json.Serialization;
using System.Security.Policy;
using System.Xml;
using System.Xml.Linq;
using static HSR_SIM_GUI.WarGear;

namespace HSR_SIM_GUI
{
    public partial class WarGear : Form
    {
        private Character mainCharacter = null;


        private static Form ShowLabelDialog(string input, string caption)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = caption;
            inputBox.StartPosition = FormStartPosition.CenterScreen;

            System.Windows.Forms.Label textBox = new Label();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);


            return inputBox;
        }


        private static DialogResult ShowInputDialog(ref string input, string caption)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = caption;
            inputBox.StartPosition = FormStartPosition.CenterScreen;

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }

        private static DialogResult showCbDialogResult(ref int index, string caption, List<Character> characters)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = caption;
            inputBox.StartPosition = FormStartPosition.CenterScreen;

            System.Windows.Forms.ComboBox cbBox = new ComboBox();
            cbBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            cbBox.Location = new System.Drawing.Point(5, 5);
            foreach (var character in characters)
            {
                cbBox.Items.Add(character.Name);
            }

            cbBox.SelectedIndex = 0;
            inputBox.Controls.Add(cbBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            index = cbBox.SelectedIndex;
            return result;
        }

        public WarGear()
        {

            InitializeComponent();
            Utils.ApplyDarkLightTheme(this);

        }

        private void WarGear_Load(object sender, EventArgs e)
        {

        }


        public class Player
        {
            public string Uid;
            public string Nickname;
            public int Level;

        }

        public class Attribute
        {
            public string field;
            public double value;
            public bool percent;

        }

        public class Skill
        {
            public string name;
            public int level;
            public int max_level;

        }

        public class Gear
        {
            public string name;
            public Attribute main_affix;
            public List<Attribute> sub_affix;
        }
        public class LCone
        {
            public string name;
            public int rank;
            public int level;
            public int promotion;

            public List<Attribute> attributes;
            public List<Attribute> properties;
        }
        public class GearSet
        {
            public string name;
            public int num;
            public List<Attribute> properties;
        }
        public class Character
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public int Rank { get; set; }
            public int Promotion
            {
                get;
                set;
            }

            public LCone light_cone;
            public List<Attribute> attributes;
            public List<Attribute> properties;
            public List<GearSet> relic_sets;
            public List<Gear> relics;

            public List<Skill> skills;
        }
        public class ApiData
        {
            public Player player;
            public List<Character> characters;
        }

        private void FillAttributes(DataGridView view, List<Attribute> attributes)
        {
            view.Rows.Clear();
            if (attributes is null)
                return;
            if (attributes.Count <= 0)
                return;


            view.Rows.Add(attributes.Count);
            foreach (Attribute attr in attributes)
            {

                view.Rows[attributes.IndexOf(attr)].Cells[0].Value = attr.field;
                view.Rows[attributes.IndexOf(attr)].Cells[1].Value = attr.value.ToString();
            }

        }
        /// <summary>
        /// Load character from object
        /// </summary>
        /// <param name="character"></param>
        private void LoadCharacter()
        {

            txtLvl.Value = mainCharacter.Level;
            txtRank.Value = mainCharacter.Rank;
            AvatarBox.Image = new Bitmap(HSR_SIM_LIB.Utils.LoadBitmap("Character\\" + mainCharacter.Name), new Size(AvatarBox.Width, AvatarBox.Height));
            label1.Text = mainCharacter.Name;
            txtLC.Text = mainCharacter.light_cone.name;
            txtLCRank.Value = mainCharacter.light_cone.rank;
            txtLcLevel.Value = mainCharacter.light_cone.level;

            FillAttributes(dgStats, mainCharacter.attributes);

            dgSets.Rows.Clear();
            if (mainCharacter.relic_sets is { Count: > 0 })
            {
                dgSets.Rows.Add(mainCharacter.relic_sets.Count);
                foreach (GearSet ger in mainCharacter.relic_sets)
                {
                    dgSets.Rows[mainCharacter.relic_sets.IndexOf(ger)].Cells[0].Value = ger.name;
                    dgSets.Rows[mainCharacter.relic_sets.IndexOf(ger)].Cells[1].Value = ger.num.ToString();
                }
            }







            //skills
            dgSkills.Rows.Clear();
            if (mainCharacter.skills.Count > 0)
                dgSkills.Rows.Add(mainCharacter.skills.Count);
            foreach (Skill skl in mainCharacter.skills)
            {

                dgSkills.Rows[mainCharacter.skills.IndexOf(skl)].Cells[0].Value = skl.name;
                dgSkills.Rows[mainCharacter.skills.IndexOf(skl)].Cells[1].Value = skl.level.ToString();
                dgSkills.Rows[mainCharacter.skills.IndexOf(skl)].Cells[2].Value = skl.max_level.ToString();
            }



        }


        private void ConcatAndRenameAttrib(List<Attribute> lst_out, List<Attribute> lst_in)
        {
            foreach (Attribute a in lst_in)
            {
                a.field += a.percent ? "_prc" : "_fix";
            }

            if (lst_in == lst_out)
                return;
            foreach (Attribute a in lst_in)
            {
                if (lst_out.Any(x => x.field == a.field))
                {
                    lst_out.First(x => x.field == a.field).value += a.value;
                }
                else
                {
                    lst_out.Add(new Attribute() { field = a.field, percent = a.percent, value = a.value });
                }
            }
        }

        private void ConcatOneAttrib(List<Attribute> lst_out, Attribute a)
        {
            a.field += a.percent ? "_prc" : "_fix";


            if (lst_out.Any(x => x.field == a.field))
            {
                lst_out.First(x => x.field == a.field).value += a.value;
            }
            else
            {
                lst_out.Add(new Attribute() { field = a.field, percent = a.percent, value = a.value });
            }

        }
        /// <summary>
        /// import from json API
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Import_Click(object sender, EventArgs e)
        {


            string input = IniF.IniReadValue("WarGear", "UID");
            DialogResult dRes = ShowInputDialog(ref input, "UID?");

            if (dRes == DialogResult.OK)
            {
                IniF.IniWriteValue("WarGear", "UID", input);
                //get from api
                Form waitDialog = ShowLabelDialog("loading data...", "please wait");
                waitDialog.Show();
                using (HttpClient wc = new HttpClient())
                {
                    var result = await wc.GetStringAsync(String.Format("https://api.mihomo.me/sr_info_parsed/{0:s}?lang={1:s}", input, "en"));


                    ApiData data = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiData>(result);

                    //add _prc to attribute names and concatinate all attributes
                    foreach (Character character in data.characters)
                    {
                        //concatinate and renaming stats
                        ConcatAndRenameAttrib(character.attributes, character.properties);
                        //delete low num sets
                        List<GearSet> filtredSet=new List<GearSet>();
                        foreach (GearSet set in  character.relic_sets)
                        {
                            if (filtredSet.All(x => x.name != set.name))
                            {
                                filtredSet.Add(set);
                            }
                            else
                            {
                                GearSet fndgs = filtredSet.First(x => x.name == set.name);
                                fndgs.num = Math.Max(fndgs.num, set.num);
                            }
                        }
                        character.relic_sets=filtredSet;
                    }

                    int index = 0;
                    waitDialog.Close();
                    waitDialog.Dispose();
                    //if not autosave
                    if (!chAutoSave.Checked)
                    {
                        if (showCbDialogResult(ref index, "Choose character", data.characters) == DialogResult.OK)
                        {
                            mainCharacter = data.characters[index];
                            LoadCharacter();

                        }
                    }
                    else
                    {
                        //save all by default names
                        foreach (Character character in data.characters)
                        {
                            XmlSave(character, GetWarGearPath() + GetDefaultFileName(character));
                        }

                        XElement profile = new XElement("Profile");
                        XElement party = new XElement("Party");
                        profile.Add(party);

                        foreach (Character character in data.characters)
                        {
                            XElement unit = new XElement("Unit");
                            unit.SetAttributeValue("template", "Character\\" + character.Name);
                            unit.SetAttributeValue("wargear", GetDefaultFileName(character, false));
                            party.Add(unit);
                        }

                        profile.Save(GetProfilePath() + String.Format("UID_{0:s}.xml", IniF.IniReadValue("WarGear", "UID")));
                        // Code to write the stream goes here.
                    }

                }

            }


        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgStats_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            int rw = e.RowIndex;
            string val = ((DataGridView)sender).Rows[e.RowIndex].Cells[1].Value?.ToString();
            string prop = ((DataGridView)sender).Rows[e.RowIndex].Cells[0].Value?.ToString();

            if (prop != null)
            {
                var charVal = mainCharacter.GetType().GetProperty(prop)?.GetValue(mainCharacter, null);
                if (charVal is int)
                {
                    mainCharacter.GetType().GetProperty(prop)?.SetValue(mainCharacter, int.Parse(val));
                }
                else if (charVal is double)
                {
                    mainCharacter.GetType().GetProperty(prop)?.SetValue(mainCharacter, double.Parse(val));
                }
                else
                {
                    mainCharacter.GetType().GetProperty(prop)?.SetValue(mainCharacter, val);
                }
            }

        }

        private void dgStats_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtLvl_ValueChanged(object sender, EventArgs e)
        {
            mainCharacter.Level = (int)((NumericUpDown)sender).Value;
        }

        private void txtRank_ValueChanged(object sender, EventArgs e)
        {
            mainCharacter.Rank = (int)((NumericUpDown)sender).Value;
        }


        private string GetWarGearPath()
        {
            return HSR_SIM_LIB.Utils.DataFolder + "\\WarGear\\";

        }
        private string GetProfilePath()
        {
            return HSR_SIM_LIB.Utils.DataFolder + "\\Profile\\";

        }

        private string GetDefaultFileName(Character character, bool withExt = true)
        {
            string ext = withExt ? ".xml" : "";
            return String.Format("{0:s}_{1:s}" + ext, character?.Name,
                IniF.IniReadValue("WarGear", "UID"));
        }
        private void SaveXML_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "xml file (*.xml)|*.xml";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.InitialDirectory = Path.GetFullPath(GetWarGearPath());
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = GetDefaultFileName(mainCharacter);
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                XmlSave(mainCharacter, saveFileDialog1.FileName);



            }
        }

        private void XmlSave(Character character, string savePath)
        {
            XElement unit = new XElement("Unit");
            unit.SetAttributeValue("name", character?.Name);
            unit.SetAttributeValue("level", character.Level.ToString());
            unit.SetAttributeValue("rank", character.Rank.ToString());
            XElement stat = new XElement("Stats");
            foreach (Attribute attr in character.attributes)
            {

                stat.SetAttributeValue(attr.field, attr.value);

            }
            unit.Add(stat);

            XElement xLc = new XElement("LightCone");
            xLc.SetAttributeValue("rank", character.light_cone.rank.ToString());
            xLc.SetAttributeValue("level", character.light_cone.level.ToString());
            xLc.SetAttributeValue("name", character.light_cone.name);
            unit.Add(xLc);

            XElement skills = new XElement("Skills");
            foreach (Skill skl in character.skills)
            {
                XElement skill = new XElement("Skill");
                skill.SetAttributeValue("name", skl.name);
                skill.SetAttributeValue("level", skl.level.ToString());
                skill.SetAttributeValue("max_level", skl.max_level.ToString());
                skills.Add(skill);

            }
            unit.Add(skills);

            XElement sets = new XElement("RelicSets");
            foreach (GearSet gearSet in character.relic_sets)
            {
                XElement set = new XElement("Set");
                set.SetAttributeValue("name", gearSet.name);
                set.SetAttributeValue("num", gearSet.num.ToString());
                sets.Add(set);

            }
            unit.Add(sets);


            unit.Save(savePath);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile
                = new OpenFileDialog();

            openFile.Filter = "xml file (*.xml)|*.xml";
            openFile.FilterIndex = 1;
            openFile.InitialDirectory = Path.GetFullPath(GetWarGearPath());
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                mainCharacter = XmlLoad(openFile.FileName);


                LoadCharacter();
            }
        }

        private Character XmlLoad(string openFileFileName)
        {
            Character character
                = new Character();
            //Scenario
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(openFileFileName);
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {

                character.Level = int.Parse(xRoot.Attributes.GetNamedItem("level")?.Value.ToString() ?? "0");
                character.Name = xRoot.Attributes.GetNamedItem("name")?.Value.ToString();
                character.Rank = int.Parse(xRoot.Attributes.GetNamedItem("rank")?.Value.ToString() ?? "0");
                character.attributes = new List<Attribute>();
                character.skills = new List<Skill>();
                character.relic_sets = new List<GearSet>();
                //parse all items
                foreach (XmlElement xnode in xRoot)
                {
                    if (xnode.Name == "LightCone")
                    {

                        character.light_cone = new LCone();
                        character.light_cone.level = int.Parse(xnode.Attributes.GetNamedItem("level")?.Value.ToString() ?? "0");
                        character.light_cone.name = xnode.Attributes.GetNamedItem("name")?.Value.ToString();
                        character.light_cone.rank = int.Parse(xnode.Attributes.GetNamedItem("rank")?.Value.ToString() ?? "0");

                    }

                    if (xnode.Name == "Stats")
                    {
                        foreach (XmlAttribute xmlattr in xnode.Attributes)
                        {
                            Attribute attr = new Attribute();
                            attr.field = xmlattr.Name;
                            attr.value = double.Parse(xmlattr.Value.Replace(".", ","));
                            character.attributes.Add(attr);
                        }
                    }

                    if (xnode.Name == "Skills")
                    {
                        foreach (XmlElement xmlSkill in xnode)
                        {
                            Skill skl = new Skill();
                            skl.name = xmlSkill.Attributes.GetNamedItem("name")?.Value.ToString(); ;
                            skl.level = int.Parse(xmlSkill.Attributes.GetNamedItem("level")?.Value.ToString());
                            skl.max_level = int.Parse(xmlSkill.Attributes.GetNamedItem("max_level")?.Value.ToString());
                            character.skills.Add(skl);
                        }
                    }

                    if (xnode.Name == "RelicSets")
                    {
                        foreach (XmlElement xmlSet in xnode)
                        {
                            GearSet gs = new GearSet();
                            gs.name = xmlSet.Attributes.GetNamedItem("name")?.Value;
                            gs.num = int.Parse(xmlSet.Attributes.GetNamedItem("num")?.Value);
                            character.relic_sets.Add(gs);
                        }
                    }


                }


            }

            return character;
        }

        private void dgSkills_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int rw = e.RowIndex;
            string skillName = ((DataGridView)sender).Rows[e.RowIndex].Cells[0].Value?.ToString();
            int lvl = int.Parse(((DataGridView)sender).Rows[e.RowIndex].Cells[1].Value.ToString());
            int max_lvl = int.Parse(((DataGridView)sender).Rows[e.RowIndex].Cells[2].Value.ToString());
            if (skillName != null)
            {
                mainCharacter.skills.First(x => x.name == skillName).level = lvl;
                mainCharacter.skills.First(x => x.name == skillName).max_level = max_lvl;
            }
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void txtLCRank_ValueChanged(object sender, EventArgs e)
        {
            mainCharacter.light_cone.rank = (int)((NumericUpDown)sender).Value;
        }

        private void txtLcLevel_ValueChanged(object sender, EventArgs e)
        {
            mainCharacter.light_cone.level = (int)((NumericUpDown)sender).Value;
        }

        private void dgSets_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int rw = e.RowIndex;
            string setName = ((DataGridView)sender).Rows[e.RowIndex].Cells[0].Value?.ToString();
            int num = int.Parse(((DataGridView)sender).Rows[e.RowIndex].Cells[1].Value.ToString());
            if (setName != null)
            {
                mainCharacter.relic_sets.First(x => x.name == setName).num = num;
            }
        }
    }
}
