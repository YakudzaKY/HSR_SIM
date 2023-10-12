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

        }

        public class Skill
        {
            public string name;
            public int level;

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
            public List<Attribute> attributes;
            public List<Skill> skills;
        }
        public class ApiData
        {
            public Player player;
            public List<Character> characters;
        }

        /// <summary>
        /// Load character from object
        /// </summary>
        /// <param name="character"></param>
        private void LoadCharacter()
        {

            txtLvl.Value = mainCharacter.Level;
            txtRank.Value = mainCharacter.Rank;
            AvatarBox.Image = new Bitmap(HSR_SIM_LIB.Utils.LoadBitmap(mainCharacter.Name), new Size(AvatarBox.Width, AvatarBox.Height));
            label1.Text = mainCharacter.Name;

            //stats
            while (dgStats.Rows.Count > 0)
            {
                dgStats.Rows.RemoveAt(0);
            }
            dgStats.Rows.Add(mainCharacter.attributes.Count);
            foreach (Attribute attr in mainCharacter.attributes)
            {

                dgStats.Rows[mainCharacter.attributes.IndexOf(attr)].Cells[0].Value = attr.field;
                dgStats.Rows[mainCharacter.attributes.IndexOf(attr)].Cells[1].Value = attr.value.ToString();
            }
            //skills
            while (dgSkills.Rows.Count > 0)
            {
                dgSkills.Rows.RemoveAt(0);
            }
            if (mainCharacter.skills.Count>0)
                dgSkills.Rows.Add(mainCharacter.skills.Count);
            foreach (Skill skl in mainCharacter.skills)
            {

                dgSkills.Rows[mainCharacter.skills.IndexOf(skl)].Cells[0].Value = skl.name;
                dgSkills.Rows[mainCharacter.skills.IndexOf(skl)].Cells[1].Value = skl.level.ToString();
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
                            unit.SetAttributeValue("template", character.Name);
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

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
           /* int rw = e.RowIndex;
            string val = ((DataGridView)sender).Rows[e.RowIndex].Cells[1].Value?.ToString();
            string prop = ((DataGridView)sender).Rows[e.RowIndex].Cells[0].Value?.ToString();

            if (prop != null)
            {

            }*/
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
            XElement skills = new XElement("Skills");
            foreach (Skill skl in character.skills)
            {
                XElement skill = new XElement("Skill");
                skill.SetAttributeValue("name",skl.name);
                skill.SetAttributeValue("level",skl.level.ToString());
                skills.Add(skill);

            }
            unit.Add(stat);
            unit.Add(skills);
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
                mainCharacter=XmlLoad(openFile.FileName);


                LoadCharacter();
            }
        }

        private Character XmlLoad( string openFileFileName)
        {
            Character character
                = new Character();
            //Scenario
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(openFileFileName);
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                character.Level= int.Parse(xRoot.Attributes.GetNamedItem("level")?.Value.ToString()??"0");
                character.Name= xRoot.Attributes.GetNamedItem("name")?.Value.ToString();
                character.Rank= int.Parse(xRoot.Attributes.GetNamedItem("rank")?.Value.ToString()??"0");
                character.attributes = new List<Attribute>();
                character.skills = new List<Skill>();
                //parse all items
                foreach (XmlElement xnode in xRoot)
                {
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
                            skl.name = xmlSkill.Attributes.GetNamedItem("name")?.Value.ToString();;
                            skl.level = int.Parse(xmlSkill.Attributes.GetNamedItem("level")?.Value.ToString());
                            character.skills.Add(skl);
                        }
                    }
                   
                }


            }

            return character;
        }
    }
}
