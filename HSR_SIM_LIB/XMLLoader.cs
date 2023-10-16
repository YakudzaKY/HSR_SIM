using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HSR_SIM_LIB;
using HSR_SIM_LIB.Fighters;
using static HSR_SIM_LIB.Ability;
using static HSR_SIM_LIB.Check;
using static HSR_SIM_LIB.Resource;
using static HSR_SIM_LIB.Unit;

namespace HSR_SIM_LIB
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
                xDoc = new XmlDocument();
                xDoc.Load(profilePath);
                xRoot = xDoc.DocumentElement;
                if (xRoot != null)
                {

                    //parse all items
                    foreach (XmlElement xnode in xRoot)
                    {

                        if (xnode.Name == "Party")
                        {
                            Combat.CurrentScenario.Party = ExtractUnits(xnode);
                        }
                    }
                }
            }

            return Combat;
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
                lvl=SafeToIntNull(xnode.Attributes.GetNamedItem("level")?.Value?.Trim());
                if (lvl == null || lvl == searchLvl)
                {
                    unitStats.BaseMaxHp = SafeToDouble(xnode.Attributes.GetNamedItem("hp")?.Value.Trim());
                    unitStats.BaseAttack = SafeToDouble(xnode.Attributes.GetNamedItem("atk")?.Value.Trim());
                    unitStats.BaseMaxEnergy = SafeToInt(xnode.Attributes.GetNamedItem("energy")?.Value.Trim());
                    unitStats.BaseSpeed = SafeToDouble(xnode.Attributes.GetNamedItem("spd")?.Value.Trim());
                    unitStats.MaxToughness = SafeToInt(xnode.Attributes.GetNamedItem("tgh")?.Value.Trim());
                    unitStats.BaseCritChance = SafeToDouble(xnode.Attributes.GetNamedItem("crit_rate")?.Value.Trim());
                    unitStats.BaseCritDmg = SafeToDouble(xnode.Attributes.GetNamedItem("crit_dmg")?.Value.Trim());
                    unitStats.BaseDef = SafeToDoubleNull(xnode.Attributes.GetNamedItem("def")?.Value.Trim());
                    unitStats.BaseBreakDmg= SafeToDouble(xnode.Attributes.GetNamedItem("break_dmg")?.Value.Trim());
                    if (xnode.Attributes.GetNamedItem("baseActionValue") is not null)
                    {
                        unitStats.BaseActionValue =
                            SafeToInt(xnode.Attributes.GetNamedItem("baseActionValue")?.Value.Trim());
                    }
                }
            }

            return unitStats;
        }

        private static double SafeToDouble(string pStr)
        {
            if (!String.IsNullOrEmpty(pStr))
                return Double.Parse(pStr.Replace(".",","));
            return 0;
        }

        private static double? SafeToDoubleNull(string pStr)
        {
            if (!String.IsNullOrEmpty(pStr))
                return Double.Parse(pStr.Replace(".",","));
            return null;
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
                XmlDocument unitDoc = new ();
                Unit unit = new();
                //load xml by 
                string unitCode = unitNode.Attributes.GetNamedItem("template").Value.Trim();
                string[] words = unitCode.Split('\\');
                unit.FighterClassName =
                    $"HSR_SIM_LIB.Fighters.{words[0]}.{words[1].Replace(" ", "")}";
                unit.UnitType = (Unit.TypeEnm)System.Enum.Parse(typeof(Unit.TypeEnm), words[0], true);
                unit.Level = int.Parse(unitNode.Attributes.GetNamedItem("level")?.Value?.Trim() ?? "1");
                unit.Rank = int.Parse(unitNode.Attributes.GetNamedItem("rank")?.Value?.Trim() ?? "0");
                string unitFile = Utils.DataFolder + "UnitTemplates\\" + unitCode + ".xml";
                unit.Name = Path.GetFileNameWithoutExtension(unitFile);
                //override by wargear
                string warGear = unitNode.Attributes.GetNamedItem("wargear")?.Value.Trim();
                //if wargear filled but no file 
                if (!string.IsNullOrEmpty(warGear) && !File.Exists(GetWarGearFile(warGear)))
                    throw new Exception(String.Format("WarGear file {0:s} not found",warGear));
                
                if (File.Exists(GetWarGearFile(warGear??unitCode)))
                    ExctractStatsFromWargear(warGear??unitCode, unit);
                

                units.Add(unit);

            }
            return units;
        }

        private static string GetWarGearFile(string param)
        {
            return Utils.DataFolder + "WarGear\\" + param + ".xml";
        }

        private  static void ExctractStatsFromWargear(string wargear, Unit unit)
        {
            XmlDocument unitDoc = new ();
            unitDoc.Load(GetWarGearFile(wargear));
            XmlElement xRoot = unitDoc.DocumentElement;
            string unitCode = xRoot.Attributes.GetNamedItem("name").Value.Trim();
            string newLevel = xRoot.Attributes.GetNamedItem("level")?.Value.Trim();
            if (!string.IsNullOrEmpty(newLevel))
                unit.Level = SafeToInt(newLevel);
            if ((unitCode) != unit.Name)
                throw new Exception(String.Format("Looking wargear for {0:s} but loaded for {1:s}",unit.Name,unitCode));
            unit.Stats= ExctractStats(xRoot,unit.Level,unit);

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

