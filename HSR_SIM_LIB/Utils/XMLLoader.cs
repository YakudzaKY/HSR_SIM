using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Xml;
using System.Xml.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.TurnBasedClasses.PreLaunchOption;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.Utils;

public static class XMLLoader
{
    public static SimCls LoadCombatFromXml(string scenarioPath, string profilePath)
    {
        SimCls Combat = new()
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
            Combat.CurrentScenario.Name = xRoot.Attributes.GetNamedItem("name").Value;

            //parse all items
            foreach (XmlElement xnode in xRoot)
            {
                if (xnode.Name == "Fights") FillFights(xnode, Combat);
                if (xnode.Name == "Prelaunch") Combat.PreLaunch = ExtractPreLaunch(xnode);
                if (xnode.Name == "Party") Combat.CurrentScenario.Party = ExtractUnits(xnode);
                if (xnode.Name == "Special") Combat.CurrentScenario.SpecialUnits = ExtractUnits(xnode);
            }
        }

        //Profile
        if (!String.IsNullOrEmpty(profilePath)) Combat.CurrentScenario.Party = ExctractPartyFromXml(profilePath);

        return Combat;
    }

    public static List<Unit> ExctractPartyFromXml(string profilePath)
    {
        List<Unit> res = null;
        var xDoc = new XmlDocument();
        xDoc.Load(profilePath);
        var xRoot = xDoc.DocumentElement;
        if (xRoot != null)
        {
            //parse all items
            foreach (XmlElement xnode in xRoot)
                if (xnode.Name == "Party")
                    res = ExtractUnits(xnode);
        }
        else
        {
            res = new List<Unit>();
        }

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
    ///     exctract stats from xml part
    /// </summary>
    /// <param name="xnode"></param>
    /// <returns></returns>
    private static UnitStats ExctractStats(XmlElement xmlItems, int searchLvl, Unit unit)
    {
        UnitStats unitStats = new(unit);
        int? lvl;
        //parse all items
        foreach (XmlElement xnode in xmlItems.SelectNodes("Stats"))
        {
            lvl = SafeToIntNull(xnode.Attributes.GetNamedItem("level")?.Value?.Trim());
            if (lvl == null || lvl == searchLvl)
            {
                //base stats
                unitStats.BaseMaxHp = SafeToDouble(xnode.Attributes.GetNamedItem("hp")?.Value.Trim());
                unitStats.BaseAttack = SafeToDouble(xnode.Attributes.GetNamedItem("atk")?.Value.Trim());
                unitStats.BaseSpeed = SafeToDouble(xnode.Attributes.GetNamedItem("spd")?.Value.Trim());
                unitStats.MaxToughness = SafeToInt(xnode.Attributes.GetNamedItem("tgh")?.Value.Trim());
                unitStats.BaseCritChance = SafeToDouble(xnode.Attributes.GetNamedItem("crit_rate")?.Value.Trim());
                unitStats.BaseCritDmg = SafeToDouble(xnode.Attributes.GetNamedItem("crit_dmg")?.Value.Trim());
                unitStats.BaseDef = SafeToDouble(xnode.Attributes.GetNamedItem("def")?.Value.Trim());
                unitStats.BaseEffectRes = SafeToDouble(xnode.Attributes.GetNamedItem("effect_res")?.Value.Trim());
                unitStats.BaseEffectHit = SafeToDouble(xnode.Attributes.GetNamedItem("effect_hit")?.Value.Trim());
                unitStats.BaseEnergyRes = SafeToDouble(xnode.Attributes.GetNamedItem("sp_rate")?.Value.Trim());
                unitStats.BaseHealRate = SafeToDouble(xnode.Attributes.GetNamedItem("heal_rate")?.Value.Trim());
                if (xnode.Attributes.GetNamedItem("baseActionValue") is not null)
                    unitStats.LoadedBaseActionValue =
                        SafeToInt(xnode.Attributes.GetNamedItem("baseActionValue")?.Value.Trim());

                //PRC stats
                unitStats.MaxHpPrc = SafeToDouble(xnode.Attributes.GetNamedItem("hp_prc")?.Value.Trim());
                unitStats.AttackPrc = SafeToDouble(xnode.Attributes.GetNamedItem("atk_prc")?.Value.Trim());
                unitStats.CritDmgPrc = SafeToDouble(xnode.Attributes.GetNamedItem("crit_dmg_prc")?.Value.Trim());
                unitStats.BreakDmgPrc = SafeToDouble(xnode.Attributes.GetNamedItem("break_dmg_prc")?.Value.Trim());
                unitStats.SpeedPrc = SafeToDouble(xnode.Attributes.GetNamedItem("spd_prc")?.Value.Trim());
                unitStats.DefPrc = SafeToDouble(xnode.Attributes.GetNamedItem("def_prc")?.Value.Trim());
                unitStats.CritRatePrc = SafeToDouble(xnode.Attributes.GetNamedItem("crit_rate_prc")?.Value.Trim());
                unitStats.EffectHitPrc = SafeToDouble(xnode.Attributes.GetNamedItem("effect_hit_prc")?.Value.Trim());
                unitStats.EffectResPrc = SafeToDouble(xnode.Attributes.GetNamedItem("effect_res_prc")?.Value.Trim());
                unitStats.BaseEnergyResPrc = SafeToDouble(xnode.Attributes.GetNamedItem("sp_rate_prc")?.Value.Trim());
                unitStats.HealRatePrc = SafeToDouble(xnode.Attributes.GetNamedItem("heal_rate_prc")?.Value.Trim());


                //fix stats
                unitStats.MaxHpFix = SafeToDouble(xnode.Attributes.GetNamedItem("hp_fix")?.Value.Trim());
                unitStats.AttackFix = SafeToDouble(xnode.Attributes.GetNamedItem("atk_fix")?.Value.Trim());
                unitStats.SpeedFix = SafeToDouble(xnode.Attributes.GetNamedItem("spd_fix")?.Value.Trim());

                //weapon damage by element???
                foreach (var elmn in (ElementEnm[])Enum.GetValues(typeof(ElementEnm)))
                    unit.GetElemBoost(elmn).Value = SafeToDouble(xnode.Attributes
                        .GetNamedItem(elmn.ToString().ToLower() + "_dmg_prc")?.Value.Trim());
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
    /// remove special chars from string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string EscapeReplaceString(string str)
    {
        return str.Trim().Replace(" ", "").Replace("'", "").Replace("-", "").Replace(":", "");
    }

    /// <summary>
    ///     Exctract unit list from xml elemenet
    /// </summary>
    /// <param name="wave"></param>
    /// <returns></returns>
    private static List<Unit> ExtractUnits(XmlElement unitPack)
    {
        List<Unit> units = new();
        foreach (XmlElement unitNode in unitPack.SelectNodes("Unit"))
        {
            Unit unit = new();
            //load xml by 
            var unitCode = unitNode.Attributes.GetNamedItem("template").Value.Trim();
            var assemblyName = unitNode.Attributes.GetNamedItem("assembly")?.Value.Trim()??"HSR_SIM_CONTENT";
            var words = unitCode.Split('\\');
            unit.FighterClassName =
                $"{assemblyName}.Content.{words[0]}.{EscapeReplaceString(words[1])}, {assemblyName}";
            unit.UnitType = (TypeEnm)Enum.Parse(typeof(TypeEnm), words[0], true);
            unit.Level =
                int.Parse(unitNode.Attributes.GetNamedItem("level")?.Value?.Trim() ??
                          "1"); //will be overwriten by wargear if possible
            unit.Rank = int.Parse(unitNode.Attributes.GetNamedItem("rank")?.Value?.Trim() ??
                                  "0"); //will be overwriten by wargear if possible
            unit.Name = words[1];
            //override by wargear
            var warGear = unitNode.Attributes.GetNamedItem("wargear")?.Value.Trim();
            //if wargear filled but no file 
            if (!string.IsNullOrEmpty(warGear) && !File.Exists(GetWarGearFile(warGear)))
                throw new Exception(string.Format("WarGear file {0:s} not found", warGear));

            if (File.Exists(GetWarGearFile(warGear ?? unitCode)))
                ExctractWargear(warGear ?? unitCode, unit);
            else
                ExctractUnitSkillsAndGear(unitNode, unit);

            units.Add(unit);
        }

        return units;
    }

    /// <summary>
    ///     Exctract pre launch options from xml
    /// </summary>

    private static List<PreLaunchOption> ExtractPreLaunch(XmlElement preLaunchOptionsXml)
    {
        List<PreLaunchOption> preLaunchOptions = new();
        foreach (XmlElement optionNode in preLaunchOptionsXml.SelectNodes("Option"))
        {

            PreLaunchOption option = new();
            option.OptionType = (PreLaunchOptionEnm)Enum.Parse(typeof(PreLaunchOptionEnm), optionNode.Attributes.GetNamedItem("type").Value.Trim(), true);
            option.Value = SafeToDouble(optionNode.Attributes.GetNamedItem("value")?.Value.Trim());



            preLaunchOptions.Add(option);
        }

        return preLaunchOptions;
    }

    private static string GetWarGearFile(string param)
    {
        return Utl.DataFolder + "WarGear\\" + param + ".xml";
    }

    private static void ExctractUnitSkillsAndGear(XmlElement xmlElement, Unit unit)
    {

        var newLevel = xmlElement.Attributes.GetNamedItem("level")?.Value.Trim();
        var newRank = xmlElement.Attributes.GetNamedItem("rank")?.Value.Trim();
        if (!string.IsNullOrEmpty(newLevel))
            unit.Level = SafeToInt(newLevel);
        if (!string.IsNullOrEmpty(newRank))
            unit.Rank = SafeToInt(newRank);

        unit.Stats = ExctractStats(xmlElement, unit.Level, unit);

        foreach (XmlElement xmlSkill in xmlElement.SelectNodes("Skill"))
        {
            Skill skill = new()
            {
                Name = xmlSkill.Attributes.GetNamedItem("name").Value.Trim(),
                Level = int.Parse(xmlSkill.Attributes.GetNamedItem("level").Value)
            };
            unit.Skills.Add(skill);
        }


        foreach (XmlElement xmlLcone in xmlElement.SelectNodes("LightCone"))
        {
            var assemblyName = xmlLcone.Attributes.GetNamedItem("assembly")?.Value.Trim()??"HSR_SIM_CONTENT";
            unit.LightConeStringPath =
                $"{assemblyName}.Content.LightCones.{EscapeReplaceString(xmlLcone.Attributes.GetNamedItem("name").Value).Trim()}, {assemblyName}";
            unit.LightConeInitRank = int.Parse(xmlLcone.Attributes.GetNamedItem("rank").Value.Trim());
        }

        foreach (XmlElement xmlRelic in xmlElement.SelectNodes("RelicSet"))
        {
            var assemblyName = xmlRelic.Attributes.GetNamedItem("assembly")?.Value.Trim()??"HSR_SIM_CONTENT";
            KeyValuePair<string, int> newRec = new(
                $"{assemblyName}.Content.Relics.{EscapeReplaceString(xmlRelic.Attributes.GetNamedItem("name").Value)}, {assemblyName}",
                int.Parse(xmlRelic.Attributes.GetNamedItem("num").Value.Trim()));
            unit.RelicsClasses.Add(newRec);
        }


    }
    private static void ExctractWargear(string wargear, Unit unit)
    {
        XmlDocument unitDoc = new();
        unitDoc.Load(GetWarGearFile(wargear));
        var xRoot = unitDoc.DocumentElement;
        var unitCode = xRoot.Attributes.GetNamedItem("name").Value.Trim();
        ExctractUnitSkillsAndGear(xRoot, unit);
        if (unitCode != unit.Name)
            throw new Exception(string.Format("Looking wargear for {0:s} but loaded for {1:s}", unit.Name, unitCode));
    }


    /// <summary>
    ///     Parsing XML part of Fights
    /// </summary>
    /// <param name="xnode">xml segment</param>
    private static void FillFights(XmlElement xnode, SimCls Combat)
    {
        foreach (XmlElement fightXml in xnode.ChildNodes)
        {
            Fight fg = new()
            {
                Name = fightXml.Attributes.GetNamedItem("name").Value.Trim(),
                Waves = new List<Wave>()
            };

            foreach (XmlElement waveXml in fightXml.SelectNodes("Wave"))
            {
                Wave ww = new()
                {
                    Units = ExtractUnits(waveXml)
                };

                fg.Waves.Add(ww);
            }

            Combat.CurrentScenario.Fights.Add(fg);
        }
    }
}