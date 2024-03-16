using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.TurnBasedClasses.PreLaunchOption;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.Utils;

public static class XmlLoader
{
    public static SimCls LoadCombatFromXml(string scenarioPath, string profilePath)
    {
        SimCls combat = new()
        {
            CurrentStep = null,
            CurrentScenario = new Scenario
            {
                Fights = new List<Fight>()
            }
        };

        //Scenario
        XmlDocument xDoc = new();
        xDoc.Load(scenarioPath);
        var xRoot = xDoc.DocumentElement;
        if (xRoot != null)
        {
            combat.CurrentScenario.Name =
                $"{xRoot.Attributes.GetNamedItem("name")?.Value} scenario: {Path.GetFileNameWithoutExtension(scenarioPath)} profile: {Path.GetFileNameWithoutExtension(profilePath)}";

            //parse all items
            foreach (XmlElement node in xRoot)
                switch (node.Name)
                {
                    case "Fights":
                        FillFights(node, combat);
                        break;
                    // ReSharper disable once StringLiteralTypo
                    case "Prelaunch":
                        combat.PreLaunch = ExtractPreLaunch(node);
                        break;
                    case "Party":
                        combat.CurrentScenario.Party = ExtractUnits(node);
                        break;
                    case "Special":
                        combat.CurrentScenario.SpecialUnits = ExtractUnits(node);
                        break;
                }
        }

        //Profile
        if (!string.IsNullOrEmpty(profilePath)) combat.CurrentScenario.Party = ExtractPartyFromXml(profilePath);

        return combat;
    }

    public static List<Unit> ExtractPartyFromXml(string profilePath)
    {
        List<Unit> res = null;
        var xDoc = new XmlDocument();
        xDoc.Load(profilePath);
        var xRoot = xDoc.DocumentElement;
        if (xRoot != null)
            //parse all items
            foreach (var node in xRoot.Cast<XmlElement>().Where(node => node.Name == "Party"))
                res = ExtractUnits(node);
        else
            res = [];

        return res;
    }

    /// <summary>
    ///     Check str for null before convert to int
    /// </summary>
    /// <param name="pStr"></param>
    /// <returns></returns>
    private static int SafeToInt(string pStr)
    {
        if (pStr != null)
            return int.Parse(pStr);
        return 0;
    }

    private static int? SafeToIntNull(string pStr)
    {
        if (pStr != null)
            return int.Parse(pStr);
        return null;
    }


    /// <summary>
    ///     extract stats from xml part
    /// </summary>
    /// <returns></returns>
    private static UnitStats ExtractStats(XmlElement xmlItems, int searchLvl, Unit unit)
    {
        UnitStats unitStats = new(unit);
        //parse all items
        foreach (XmlElement node in xmlItems.SelectNodes("Stats")!)
        {
            var lvl = SafeToIntNull(node.Attributes.GetNamedItem("level")?.Value?.Trim());
            if (lvl == null || lvl == searchLvl)
            {
                //base stats
                unitStats.BaseMaxHp = SafeToDouble(node.Attributes.GetNamedItem("hp")?.Value?.Trim());
                unitStats.BaseAttack = SafeToDouble(node.Attributes.GetNamedItem("atk")?.Value?.Trim());
                unitStats.BaseSpeed = SafeToDouble(node.Attributes.GetNamedItem("spd")?.Value?.Trim());
                unitStats.MaxToughness = SafeToInt(node.Attributes.GetNamedItem("tgh")?.Value?.Trim());
                unitStats.BaseCritChance = SafeToDouble(node.Attributes.GetNamedItem("crit_rate")?.Value?.Trim());
                unitStats.BaseCritDmg = SafeToDouble(node.Attributes.GetNamedItem("crit_dmg")?.Value?.Trim());
                unitStats.BaseDef = SafeToDouble(node.Attributes.GetNamedItem("def")?.Value?.Trim());
                unitStats.BaseEnergyRes = SafeToDouble(node.Attributes.GetNamedItem("sp_rate")?.Value?.Trim());
                unitStats.BaseHealRate = SafeToDouble(node.Attributes.GetNamedItem("heal_rate")?.Value?.Trim());
                if (node.Attributes.GetNamedItem("baseActionValue") is not null)
                    unitStats.LoadedBaseActionValue =
                        SafeToInt(node.Attributes.GetNamedItem("baseActionValue")?.Value?.Trim());

                //PRC stats
                unitStats.MaxHpPrc = SafeToDouble(node.Attributes.GetNamedItem("hp_prc")?.Value?.Trim());
                unitStats.AttackPrc = SafeToDouble(node.Attributes.GetNamedItem("atk_prc")?.Value?.Trim());
                unitStats.CritDmgPrc = SafeToDouble(node.Attributes.GetNamedItem("crit_dmg_prc")?.Value?.Trim());
                unitStats.BreakDmgPrc = SafeToDouble(node.Attributes.GetNamedItem("break_dmg_prc")?.Value?.Trim());
                unitStats.SpeedPrc = SafeToDouble(node.Attributes.GetNamedItem("spd_prc")?.Value?.Trim());
                unitStats.DefPrc = SafeToDouble(node.Attributes.GetNamedItem("def_prc")?.Value?.Trim());
                unitStats.CritRatePrc = SafeToDouble(node.Attributes.GetNamedItem("crit_rate_prc")?.Value?.Trim());
                unitStats.EffectHitPrc = SafeToDouble(node.Attributes.GetNamedItem("effect_hit_prc")?.Value?.Trim());
                unitStats.EffectResPrc = SafeToDouble(node.Attributes.GetNamedItem("effect_res_prc")?.Value?.Trim());
                unitStats.BaseEnergyResPrc = SafeToDouble(node.Attributes.GetNamedItem("sp_rate_prc")?.Value?.Trim());
                unitStats.HealRatePrc = SafeToDouble(node.Attributes.GetNamedItem("heal_rate_prc")?.Value?.Trim());


                //fix stats
                unitStats.MaxHpFix = SafeToDouble(node.Attributes.GetNamedItem("hp_fix")?.Value?.Trim());
                unitStats.AttackFix = SafeToDouble(node.Attributes.GetNamedItem("atk_fix")?.Value?.Trim());
                unitStats.SpeedFix = SafeToDouble(node.Attributes.GetNamedItem("spd_fix")?.Value?.Trim());

                //weapon damage by element???
                foreach (var elm in (Ability.ElementEnm[])Enum.GetValues(typeof(Ability.ElementEnm)))
                    unit.GetElemBoost(elm).Value = SafeToDouble(node.Attributes
                        .GetNamedItem(elm.ToString().ToLower() + "_dmg_prc")?.Value?.Trim());
            }
        }

        return unitStats;
    }

    private static double SafeToDouble(string pStr)
    {
        if (!string.IsNullOrEmpty(pStr))
            return double.Parse(pStr.Replace(".", ","));
        return 0;
    }


    /// <summary>
    ///     remove special chars from string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string EscapeReplaceString(string str)
    {
        return str.Trim().Replace(" ", "").Replace("'", "").Replace("-", "").Replace(":", "");
    }

    /// <summary>
    ///     Extract unit list from xml element
    /// </summary>
    /// <returns></returns>
    private static List<Unit> ExtractUnits(XmlNode unitPack)
    {
        List<Unit> units = [];
        foreach (XmlElement unitNode in unitPack.SelectNodes("Unit")!)
        {
            Unit unit = new();
            var unitCode = unitNode.Attributes.GetNamedItem("template")?.Value?.Trim();
            if (unitCode is null)
                throw new Exception("Unit template value is null");
            var assemblyName = unitNode.Attributes.GetNamedItem("assembly")?.Value?.Trim() ?? "HSR_SIM_CONTENT";
            var words = unitCode.Split('\\');
            unit.UnitType = (TypeEnm)Enum.Parse(typeof(TypeEnm), words[0], true);
            unit.Level =
                int.Parse(unitNode.Attributes.GetNamedItem("level")?.Value?.Trim() ??
                          "1"); //will be overwritten by wargear if possible
            unit.Rank = int.Parse(unitNode.Attributes.GetNamedItem("rank")?.Value?.Trim() ??
                                  "0"); //will be overwritten by wargear if possible
            unit.Name = words[1];
            //override by wargear
            var warGear = unitNode.Attributes.GetNamedItem("wargear")?.Value?.Trim();
            //if wargear filled but no file 
            if (!string.IsNullOrEmpty(warGear) && !File.Exists(GetWarGearFile(warGear)))
                throw new Exception($"WarGear file {warGear} not found");

            if (File.Exists(GetWarGearFile(warGear ?? unitCode)))
                ExtractWargear(warGear ?? unitCode, unit);
            else
                ExtractUnitSkillsAndGear(unitNode, unit);
            unit.FighterClassName =
                $"{assemblyName}.Content.{words[0]}.{EscapeReplaceString(words[1])}, {assemblyName}";
            units.Add(unit);
        }

        return units;
    }

    /// <summary>
    ///     Extract pre launch options from xml
    /// </summary>
    private static List<PreLaunchOption> ExtractPreLaunch(XmlElement preLaunchOptionsXml)
    {
        List<PreLaunchOption> preLaunchOptions = [];
        foreach (XmlElement optionNode in preLaunchOptionsXml.SelectNodes("Option")!)
        {
            PreLaunchOption option = new()
            {
                OptionType = (PreLaunchOptionEnm)Enum.Parse(typeof(PreLaunchOptionEnm),
                    optionNode.Attributes.GetNamedItem("type")?.Value?.Trim() ?? "", true),
                Value = SafeToDouble(optionNode.Attributes.GetNamedItem("value")?.Value?.Trim())
            };


            preLaunchOptions.Add(option);
        }

        return preLaunchOptions;
    }

    private static string GetWarGearFile(string param)
    {
        return Utl.DataFolder + "WarGear\\" + param + ".xml";
    }

    private static void ExtractUnitSkillsAndGear(XmlElement xmlElement, Unit unit)
    {
        var newLevel = xmlElement.Attributes.GetNamedItem("level")?.Value?.Trim();
        var newRank = xmlElement.Attributes.GetNamedItem("rank")?.Value?.Trim();
        if (!string.IsNullOrEmpty(newLevel))
            unit.Level = SafeToInt(newLevel);
        if (!string.IsNullOrEmpty(newRank))
            unit.Rank = SafeToInt(newRank);

        unit.Stats = ExtractStats(xmlElement, unit.Level, unit);

        foreach (XmlElement xmlSkill in xmlElement.SelectNodes("Skill")!)
        {
            Skill skill = new()
            {
                Name = xmlSkill.Attributes.GetNamedItem("name")?.Value?.Trim(),
                Level = int.Parse(xmlSkill.Attributes.GetNamedItem("level")?.Value ?? "0")
            };
            unit.Skills.Add(skill);
        }


        foreach (XmlElement xmlLCone in xmlElement.SelectNodes("LightCone")!)
        {
            var assemblyName = xmlLCone.Attributes.GetNamedItem("assembly")?.Value?.Trim() ?? "HSR_SIM_CONTENT";
            unit.LightConeStringPath =
                $"{assemblyName}.Content.LightCones.{EscapeReplaceString(xmlLCone.Attributes.GetNamedItem("name")?.Value).Trim()}, {assemblyName}";
            unit.LightConeInitRank = int.Parse(xmlLCone.Attributes.GetNamedItem("rank")?.Value?.Trim() ?? "0");
        }

        foreach (XmlElement xmlRelic in xmlElement.SelectNodes("RelicSet")!)
        {
            var assemblyName = xmlRelic.Attributes.GetNamedItem("assembly")?.Value?.Trim() ?? "HSR_SIM_CONTENT";
            KeyValuePair<string, int> newRec = new(
                $"{assemblyName}.Content.Relics.{EscapeReplaceString(xmlRelic.Attributes.GetNamedItem("name")?.Value)}, {assemblyName}",
                int.Parse(xmlRelic.Attributes.GetNamedItem("num")?.Value?.Trim() ?? "0"));
            unit.RelicsClasses.Add(newRec);
        }
    }

    private static void ExtractWargear(string wargear, Unit unit)
    {
        XmlDocument unitDoc = new();
        unitDoc.Load(GetWarGearFile(wargear));
        var xRoot = unitDoc.DocumentElement;
        if (xRoot == null) return;
        var unitCode = xRoot.Attributes.GetNamedItem("name")!.Value!.Trim();
        ExtractUnitSkillsAndGear(xRoot, unit);
        if (unitCode != unit.Name)
            throw new Exception($"Looking wargear for {unit.Name} but loaded for {unitCode}");
    }


    /// <summary>
    ///     Parsing XML part of Fights
    /// </summary>
    /// <param name="xnode">xml segment</param>
    /// <param name="combat"></param>
    private static void FillFights(XmlElement xnode, SimCls combat)
    {
        foreach (XmlElement fightXml in xnode.ChildNodes)
        {
            Fight fg = new()
            {
                Name = fightXml.Attributes.GetNamedItem("name")?.Value?.Trim(),
                Waves = new List<Wave>()
            };

            foreach (XmlElement waveXml in fightXml.SelectNodes("Wave")!)
            {
                Wave ww = new()
                {
                    Units = ExtractUnits(waveXml)
                };

                fg.Waves.Add(ww);
            }

            combat.CurrentScenario.Fights.Add(fg);
        }
    }
}