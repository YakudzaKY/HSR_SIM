using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Fighters.LightCones;
using HSR_SIM_LIB.Fighters.Relics;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.Utils
{
    public static class XMLLoader
    {
        public static SimCls LoadCombatFromXml(string scenarioPath, string profilePath)
        {

            SimCls Combat = new()
            {
                CurrentStep = null,
                CurrentScenario = new()
                {
                    Fights = new List<Fight>()
                }
            };

            //Scenario
            XmlDocument xDoc = new();
            xDoc.Load(scenarioPath);
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                Combat.CurrentScenario.Name = xRoot.Attributes.GetNamedItem("name").Value;

                //parse all items
                foreach (XmlElement xnode in xRoot)
                {
                    if (xnode.Name == "Fights")
                    {
                        FillFights(xnode, Combat);
                    }
                    if (xnode.Name == "Party")
                    {
                        Combat.CurrentScenario.Party = ExtractUnits(xnode);
                    }

                    if (xnode.Name == "Special")
                    {
                        Combat.CurrentScenario.SpecialUnits = ExtractUnits(xnode);
                    }
                }


            }
            //Profile
            if (profilePath != null)
            {
                Combat.CurrentScenario.Party = ExctractPartyFromXml(profilePath);
            }

            return Combat;
        }

        public static List<Unit> ExctractPartyFromXml(string profilePath)
        {
            List<Unit> res=null;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(profilePath);
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {

                //parse all items
                foreach (XmlElement xnode in xRoot)
                {

                    if (xnode.Name == "Party")
                    {
                        res= ExtractUnits(xnode);
                    }
                }
            }
            else
            {
                res=new List<Unit>();
            }

            return res;
        }

        /// <summary>
        /// Check str for null before convert to int
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
        /// exctract stats from xml part
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
                    {
                        unitStats.LoadedBaseActionValue =
                            SafeToInt(xnode.Attributes.GetNamedItem("baseActionValue")?.Value.Trim());
                    }

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
                    foreach (ElementEnm elmn in (ElementEnm[])Enum.GetValues(typeof(ElementEnm)))
                    {
                        unit.GetElemBoost(elmn).Value = SafeToDouble(xnode.Attributes.GetNamedItem(elmn.ToString().ToLower() + "_dmg_prc")?.Value.Trim());
                    }

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
        /// Exctract unit list from xml elemenet
        /// </summary>
        /// <param name="wave"></param>
        /// <returns></returns>
        private static List<Unit> ExtractUnits(XmlElement unitPack)
        {
            List<Unit> units = new();
            foreach (XmlElement unitNode in unitPack.SelectNodes("Unit"))
            {
                XmlDocument unitDoc = new();
                Unit unit = new();
                //load xml by 
                string unitCode = unitNode.Attributes.GetNamedItem("template").Value.Trim();
                string[] words = unitCode.Split('\\');
                unit.FighterClassName =
                    $"HSR_SIM_LIB.Fighters.{words[0]}.{words[1].Replace(" ", "")}";
                unit.UnitType = (TypeEnm)Enum.Parse(typeof(TypeEnm), words[0], true);
                unit.Level = int.Parse(unitNode.Attributes.GetNamedItem("level")?.Value?.Trim() ?? "1");//will be overwriten by wargear if possible
                unit.Rank = int.Parse(unitNode.Attributes.GetNamedItem("rank")?.Value?.Trim() ?? "0");//will be overwriten by wargear if possible
                string unitFile = Utl.DataFolder + "UnitTemplates\\" + unitCode + ".xml";
                unit.Name = Path.GetFileNameWithoutExtension(unitFile);
                //override by wargear
                string warGear = unitNode.Attributes.GetNamedItem("wargear")?.Value.Trim();
                //if wargear filled but no file 
                if (!string.IsNullOrEmpty(warGear) && !File.Exists(GetWarGearFile(warGear)))
                    throw new Exception(string.Format("WarGear file {0:s} not found", warGear));

                if (File.Exists(GetWarGearFile(warGear ?? unitCode)))
                    ExctractWargear(warGear ?? unitCode, unit);


                units.Add(unit);

            }
            return units;
        }

        private static string GetWarGearFile(string param)
        {
            return Utl.DataFolder + "WarGear\\" + param + ".xml";
        }

        private static void ExctractWargear(string wargear, Unit unit)
        {
            XmlDocument unitDoc = new();
            unitDoc.Load(GetWarGearFile(wargear));
            XmlElement xRoot = unitDoc.DocumentElement;
            string unitCode = xRoot.Attributes.GetNamedItem("name").Value.Trim();
            string newLevel = xRoot.Attributes.GetNamedItem("level")?.Value.Trim();
            string newRank = xRoot.Attributes.GetNamedItem("rank")?.Value.Trim();
            if (!string.IsNullOrEmpty(newLevel))
                unit.Level = SafeToInt(newLevel);
            if (!string.IsNullOrEmpty(newRank))
                unit.Rank = SafeToInt(newRank);
            if (unitCode != unit.Name)
                throw new Exception(string.Format("Looking wargear for {0:s} but loaded for {1:s}", unit.Name, unitCode));
            unit.Stats = ExctractStats(xRoot, unit.Level, unit);

            if (unit.Fighter is DefaultFighter)
            {
               
                foreach (XmlElement xmlLcone in xRoot.SelectNodes("LightCone"))
                {
                    unit.LightConeStringPath = $"HSR_SIM_LIB.Fighters.LightCones.Cones.{xmlLcone.Attributes.GetNamedItem("name").Value.Trim().Replace(" ", "").Replace("-", "").Replace(":", "")}";
                    unit.LightConeInitRank = int.Parse(xmlLcone.Attributes.GetNamedItem("rank").Value.Trim());
                }

                foreach (XmlElement xmlRelic in xRoot.SelectNodes("RelicSet"))
                {
                    KeyValuePair<string, int> newRec = new (
                        $"HSR_SIM_LIB.Fighters.Relics.Set.{xmlRelic.Attributes.GetNamedItem("name").Value.Trim().Replace(" ", "").Replace("-", "").Replace(":", "")}",
                        int.Parse(xmlRelic.Attributes.GetNamedItem("num").Value.Trim()));
                    unit.RelicsClasses.Add(newRec);
                }

                foreach (XmlElement xmlSkill in xRoot.SelectNodes("Skill"))
                {

                    Skill skill = new() { Name = xmlSkill.Attributes.GetNamedItem("name").Value.Trim(), Level = int.Parse(xmlSkill.Attributes.GetNamedItem("level").Value) };
                   unit.Skills.Add(skill);

                }
            }

        }


        /// <summary>
        /// Parsing XML part of Fights
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
}

