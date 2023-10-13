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

            SimCls Combat = new SimCls();
            Combat.CurrentStep = null;
            Combat.CurrentScenario = new Scenario();

            Combat.CurrentScenario.Fights = new List<Fight>();

            //Scenario
            XmlDocument xDoc = new XmlDocument();
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
        private static bool? SafeToBoolNull(string pStr)
        {
            if (pStr != null)
                return bool.Parse(pStr);
            return null;
        }

        private static bool SafeToBool(string pStr)
        {
            if (pStr != null)
                return bool.Parse(pStr);
            return false;
        }


        /// <summary>
        /// exctract stats from xml part
        /// </summary>
        /// <param name="xnode"></param>
        /// <returns></returns>
        private static UnitStats ExctractStats(XmlElement xmlItems, int searchLvl)
        {
            UnitStats unitStats = new UnitStats();
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
            if (pStr != null)
                return Double.Parse(pStr.Replace(".",","));
            return 0;
        }

        /// <summary>
        /// Exctract value from first tag by the tagname
        /// </summary>
        /// <param name="xnode"></param>
        /// <returns></returns>
        private static string getXmlTagValue(XmlElement xnode, string tag)
        {
            foreach (XmlElement item in xnode.SelectNodes(tag))
            {
                return item.InnerText;
            }
            return null;
        }


        /// <summary>
        /// Exctract abilities(is not mandatory in xml)
        /// </summary>
        /// <param name="xmlItems"></param>
        /// <returns></returns>
        private static List<Ability> ExctractAbilities(XmlElement xmlItems, Unit parent)
        {
            List<Ability> abilities = new List<Ability>();
            foreach (XmlElement abilitiyXml in xmlItems.SelectNodes("Ability"))
            {

                Ability ability = new Ability(parent);
                ability.AbilityType = (AbilityTypeEnm)System.Enum.Parse(typeof(AbilityTypeEnm), abilitiyXml.Attributes.GetNamedItem("type").Value.Trim(), true);
                ability.Name = abilitiyXml.Attributes.GetNamedItem("name").Value.Trim();
                ability.CostType = (ResourceType)System.Enum.Parse(typeof(ResourceType), abilitiyXml.Attributes.GetNamedItem("costType").Value.Trim(),true);
                ability.Cost = (short)int.Parse(abilitiyXml.Attributes.GetNamedItem("cost").Value.Trim());
                if (abilitiyXml.Attributes.GetNamedItem("element") != null)
                    ability.Element = (ElementEnm)System.Enum.Parse(typeof(ElementEnm),
                        abilitiyXml.Attributes.GetNamedItem("element")?.Value?.Trim(), true);
                else
                    ability.Element = parent.Element;
                //events
                foreach (XmlElement xmlevent in abilitiyXml.SelectNodes("Event"))
                {
                    Event ent = new Event()
                    {
                        Type = (Event.EventType)System.Enum.Parse(typeof(Event.EventType), xmlevent.Attributes.GetNamedItem("type").Value.Trim(), true)
                        ,
                        OnStepType = (Step.StepTypeEnm)System.Enum.Parse(typeof(Step.StepTypeEnm), xmlevent.Attributes.GetNamedItem("onStep").Value.Trim(), true)
                        ,
                        NeedCalc = SafeToBool(xmlevent.Attributes.GetNamedItem("needCalc")?.Value?.Trim())
                        ,
                        ResType = (ResourceType)Enum.Parse(typeof(ResourceType), xmlevent.Attributes.GetNamedItem("resType")?.Value?.Trim() ?? ResourceType.nil.ToString(),true)
                        ,
                        StrValue = xmlevent.Attributes.GetNamedItem("strValue")?.Value?.Trim()
                        ,
                        TargetType = (TargetTypeEnm)Enum.Parse(typeof(TargetTypeEnm), xmlevent.Attributes.GetNamedItem("target")?.Value?.Trim() ?? TargetTypeEnm.Self.ToString(), true)
                        ,
                        CanSetToZero= SafeToBool(xmlevent.Attributes.GetNamedItem("canSetToZero")?.Value?.Trim())
                        ,
                        AbilityValue = ability


                    };
                    

                    foreach (XmlElement xmlmod in xmlevent.SelectNodes("Mod"))
                    {
                        Mod mod = new Mod()
                        {
                            Type = (Mod.ModType)System.Enum.Parse(typeof(Mod.ModType), xmlmod.Attributes.GetNamedItem("type").Value.Trim(),true)
                            ,
                            Target = (Mod.ModTarget)System.Enum.Parse(typeof(Mod.ModTarget), xmlmod.Attributes.GetNamedItem("target").Value.Trim(), true)
                            ,
                            Modifier = (Mod.ModifierType)System.Enum.Parse(typeof(Mod.ModifierType), xmlmod.Attributes.GetNamedItem("modifier").Value.Trim(), true)
                            ,
                            Value = int.Parse(xmlmod.Attributes.GetNamedItem("value").Value?.Trim())
                            ,
                            Duration = SafeToIntNull(xmlmod.Attributes.GetNamedItem("duration")?.Value?.Trim())
                            ,
                            Dispellable = SafeToBoolNull(xmlmod.Attributes.GetNamedItem("dispellable")?.Value?.Trim())
                        };
                        ent.Mods.Add(mod);
                    }

                    FillExecuteWhenFromXml(xmlevent,ent.ExecuteWhen);
                    ability.Events.Add(ent);

                }

                FillExecuteWhenFromXml(abilitiyXml,ability.ExecuteWhen);
                

                abilities.Add(ability);


            }
            return abilities;
        }

        private static void FillExecuteWhenFromXml(XmlElement abilitiyXml, List<Condition> executeWhen)
        {
            //execute conditions by tags
            foreach (XmlElement xmlExecute in abilitiyXml.SelectNodes("ExecuteWhen"))
            {
                //execute conditions by template
                string template = xmlExecute.Attributes.GetNamedItem("template")?.Value.Trim();
                if (template != null)
                {
                    XmlDocument tempalteDoc = new XmlDocument();
                    tempalteDoc.Load(Utils.DataFolder + "ExecuteTemplates\\" + template + ".xml");
                    XmlElement templateRoot = tempalteDoc.DocumentElement;
                    FillExecuteWhen(templateRoot, executeWhen);
                }
                else
                    FillExecuteWhen(xmlExecute, executeWhen);
            }
        }

        /// <summary>
        /// parse ExecuteWhen->Condition
        /// </summary>
        /// <param name="xmlExecute"></param>
        /// <param name="ability"></param>
        private static void FillExecuteWhen(XmlElement xmlExecute, List<Condition> executeWhen)
        {
            foreach (XmlElement xmlCondition in xmlExecute.SelectNodes("Condition"))
            {
                Condition condition = new Condition();
                condition.OrGroup = xmlCondition.Attributes.GetNamedItem("orgroup")?.Value.Trim();
                FillConditionChecks(xmlCondition, condition);

                executeWhen.Add(condition);
            }
        }
        /// <summary>
        /// parse Condition->Check
        /// </summary>
        /// <param name="xmlCondition"></param>
        /// <param name="condition"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void FillConditionChecks(XmlElement xmlCondition, Condition condition)
        {
            foreach (XmlElement xmlCheck in xmlCondition.SelectNodes("Check"))
            {

                condition.Checks.Add(ExctractCheck(xmlCheck));
            }
        }

        /// <summary>
        /// Exctract Check
        /// </summary>
        /// <param name="xmlCheck"></param>
        /// <returns></returns>
        private static Check ExctractCheck(XmlElement xmlCheck)
        {
            Check check = new Check();
            //parse all attributes 
            foreach (XmlAttribute xmlAttrib in xmlCheck.Attributes)
            {

                if (String.Equals(xmlAttrib.Name, "type", StringComparison.OrdinalIgnoreCase))
                    check.CheckType = (CheckTypeEnm)System.Enum.Parse(typeof(CheckTypeEnm), xmlAttrib.Value.Trim(), true);
                else if (String.Equals(xmlAttrib.Name, "value", StringComparison.OrdinalIgnoreCase))
                    check.Value = xmlAttrib.Value.Trim();
                else if (String.Equals(xmlAttrib.Name, "clause", StringComparison.OrdinalIgnoreCase))
                    check.Clause = bool.Parse(xmlAttrib.Value.Trim());
                else
                    throw new NotImplementedException();

            }

            foreach (XmlElement xmlInnerCheck in xmlCheck.SelectNodes("Check"))
            {
                check.InnerChecks.Add(ExctractCheck(xmlInnerCheck));
            }

            return check;

        }
        /// <summary>
        /// Exctract unit list from xml elemenet
        /// </summary>
        /// <param name="wave"></param>
        /// <returns></returns>
        private static List<Unit> ExtractUnits(XmlElement unitPack)
        {
            List<Unit> units = new List<Unit>();
            foreach (XmlElement unitNode in unitPack.SelectNodes("Unit"))
            {
                XmlDocument unitDoc = new XmlDocument();
                Unit unit = new Unit();
                //load xml by 
                string unitCode = unitNode.Attributes.GetNamedItem("template").Value.Trim();
                unit.Level = int.Parse(unitNode.Attributes.GetNamedItem("level")?.Value?.Trim() ?? "1");
                string unitFile = Utils.DataFolder + "UnitTemplates\\" + unitCode + ".xml";
                unit.Name = Path.GetFileNameWithoutExtension(unitFile);
               
                unitDoc.Load(unitFile);
                XmlElement xRoot = unitDoc.DocumentElement;
                if (xRoot != null)
                {
                   
                    string elementVal = xRoot.Attributes.GetNamedItem("element")?.Value.Trim();
                    if (elementVal != null)
                        unit.Element = (Unit.ElementEnm)System.Enum.Parse(typeof(Unit.ElementEnm), elementVal, true);
                    unit.UnitType = (Unit.TypeEnm)System.Enum.Parse(typeof(Unit.TypeEnm), xRoot.Attributes.GetNamedItem("type").Value.Trim(), true);
                    unit.Stats = ExctractStats(xRoot, unit.Level);
                    //override by wargear
                    string wargear = unitNode.Attributes.GetNamedItem("wargear")?.Value.Trim();
                    if (!string.IsNullOrEmpty(wargear))
                    {
                        ExctractStatsFromWargear(wargear, unit);
                    }

                    unit.Abilities = ExctractAbilities(xRoot, unit);
                    unit.Weaknesses = ExctractWeaknesses(xRoot);
                    units.Add(unit);
                }

            }
            return units;
        }

        private  static void ExctractStatsFromWargear(string wargear, Unit unit)
        {
            XmlDocument unitDoc = new XmlDocument();
            unitDoc.Load(Utils.DataFolder + "WarGear\\" + wargear + ".xml");
            XmlElement xRoot = unitDoc.DocumentElement;
            string unitCode = xRoot.Attributes.GetNamedItem("name").Value.Trim();
            unit.Level = int.Parse(xRoot.Attributes.GetNamedItem("level").Value.Trim());
            if ((unitCode) != unit.Name)
                throw new Exception(String.Format("Looking wargear for {0:s} but loaded for {1:s}",unit.Name,unitCode));
            unit.Stats= ExctractStats(xRoot,unit.Level);

        }

        private static List<ElementEnm> ExctractWeaknesses(XmlElement xmlItems)
        {
            List<ElementEnm> weaknesses = new List<ElementEnm>();
            foreach (XmlElement weaknessXml in xmlItems.SelectNodes("Weakness"))
            {
                ElementEnm weakness = (ElementEnm)System.Enum.Parse(typeof(ElementEnm), weaknessXml.Attributes.GetNamedItem("type").Value.Trim(), true);
                weaknesses.Add(weakness);
            }
            return weaknesses;

        }

        /// <summary>
        /// Parsing XML part of Fights
        /// </summary>
        /// <param name="xnode">xml segment</param>
        private static void FillFights(XmlElement xnode, SimCls Combat)
        {


            foreach (XmlElement fightXml in xnode.ChildNodes)
            {
                Fight fg = new Fight();
                fg.Name = fightXml.Attributes.GetNamedItem("name").Value.Trim();
                fg.Waves = new List<Wave>();

                foreach (XmlElement waveXml in fightXml.SelectNodes("Wave"))
                {
                    Wave ww = new Wave();
                    ww.Units = ExtractUnits(waveXml);

                    fg.Waves.Add(ww);
                }
                Combat.CurrentScenario.Fights.Add(fg);
            }
        }

    }
}

