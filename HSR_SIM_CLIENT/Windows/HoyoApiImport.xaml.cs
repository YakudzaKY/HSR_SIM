using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;
using HSR_SIM_LIB.Utils;
using Newtonsoft.Json;
using static HSR_SIM_CLIENT.Utils.GuiUtils;

namespace HSR_SIM_CLIENT.Windows;

public partial class HoyoApiImport : INotifyPropertyChanged
{
    private const string ImportWait = "Import";
    private const string ImportProgress = "Please wait";
    private string btnImportCaption = ImportWait;

    public HoyoApiImport()
    {
        InitializeComponent();
    }

    public bool ApiFullSave { get; set; } = true;
    public string? ApiMyUid { get; set; } = IniF.IniReadValue("WarGear", "UID");

    public string BtnImportCaption
    {
        get => btnImportCaption;
        set
        {
            if (Equals(value, btnImportCaption)) return;
            btnImportCaption = value;

            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    ///     rename stats by values(percent or not)
    /// </summary>
    /// <param name="lst_out"></param>
    /// <param name="lst_in"></param>
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

    private async void ButtonImport_OnClick(object sender, RoutedEventArgs e)
    {
        BtnImportCaption = ImportProgress;
        IniF.IniWriteValue("WarGear", "UID", ApiMyUid);
        //get from api
        //  var waitDialog = ShowLabelDialog("loading data...", "please wait");
        // waitDialog.Show();
        using HttpClient wc = new();
        var result = await wc.GetStringAsync($"https://api.mihomo.me/sr_info_parsed/{ApiMyUid}?lang=en");


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


        //if not autosave
        /* if (!chAutoSave.Checked)
         {
             if (ShowCbDialogResult(ref index, "Choose character", data.characters) == DialogResult.OK)
             {
                 mainCharacter = data.characters[index];
                 LoadCharacter();
             }
         }
         else
         {*/
        //save all by default names
        foreach (var character in data.characters)
            XmlSave(character, GetWarGearPath() + GetDefaultFileName(character));

        XElement profile = new("Profile");
        XElement party = new("Party");
        profile.Add(party);

        foreach (var character in data.characters.Take(4))
        {
            XElement unit = new("Unit");
            unit.SetAttributeValue("template", "Character\\" + character.Name);
            unit.SetAttributeValue("wargear", GetDefaultFileName(character, false));
            party.Add(unit);
        }

        profile.Save(GetProfilePath() + string.Format("UID_{0:s}.xml", IniF.IniReadValue("WarGear", "UID")));
        // Code to write the stream goes here.
        BtnImportCaption = ImportWait;
    }

    /// <summary>
    ///     save character gear and stats into XML
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