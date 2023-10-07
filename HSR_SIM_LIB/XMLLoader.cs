using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HSR_SIM_LIB;
using static HSR_SIM_LIB.Ability;
using static HSR_SIM_LIB.Check;
using static HSR_SIM_LIB.Resource;

namespace HSR_SIM_LIB
{
    public static class XMLLoader
    {
        public static SimCls LoadCombatFromXml(string ScenarioPath)
        {

            SimCls Combat = new SimCls();
            Combat.CurrentStep = null;
            Combat.CurrentScenario = new Scenario();

            Combat.CurrentScenario.Fights = new List<Fight>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(ScenarioPath);
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
        /// <summary>
        /// exctract stats from xml part
        /// </summary>
        /// <param name="xnode"></param>
        /// <returns></returns>
        private static UnitStats ExctractStats(XmlElement xmlItems)
        {
            UnitStats unitStats = new UnitStats();
            //parse all items
            foreach (XmlElement xnode in xmlItems.SelectNodes("Stats"))
            {
                unitStats.BaseMaxHp = int.Parse(xnode.Attributes.GetNamedItem("maxHp").Value.Trim());
                unitStats.BaseAttack = int.Parse(xnode.Attributes.GetNamedItem("attack").Value.Trim());
                unitStats.BaseMaxEnergy = SafeToInt(xnode.Attributes.GetNamedItem("energy")?.Value.Trim());

            }

            return unitStats;
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
        private static List<Ability> ExctractAbilities(XmlElement xmlItems,Unit parent)
        {
            List<Ability> abilities = new List<Ability>();
            foreach (XmlElement abilitiyXml in xmlItems.SelectNodes("Ability"))
            {

                Ability ability = new Ability(parent);
                ability.AbilityType = (AbilityTypeEnm)System.Enum.Parse(typeof(AbilityTypeEnm), abilitiyXml.Attributes.GetNamedItem("type").Value.Trim());
                ability.Name = abilitiyXml.Attributes.GetNamedItem("name").Value.Trim();
                ability.CostType =  (ResourceType)System.Enum.Parse(typeof(ResourceType), abilitiyXml.Attributes.GetNamedItem("costtype").Value.Trim()) ;
                ability.Cost = (short)int.Parse( abilitiyXml.Attributes.GetNamedItem("cost").Value.Trim());
                //events
                foreach (XmlElement xmlevent in abilitiyXml.SelectNodes("Event") ) 
                {
                    Event ent = new Event() { Type = (Event.EventType)System.Enum.Parse(typeof(Event.EventType), xmlevent.Attributes.GetNamedItem("name").Value.Trim()) };
                    ability.Events.Add(ent);

                }
                //execute conditions by tags
                foreach (XmlElement xmlExecute in abilitiyXml.SelectNodes("ExecuteWhen"))
                {
                    //execute conditions by template
                    string template = xmlExecute.Attributes.GetNamedItem("template")?.Value.Trim();
                    if (template != null)
                    {
                        XmlDocument tempalteDoc= new XmlDocument();
                        tempalteDoc.Load(Utils.DataFolder + "ExecuteTemplates\\" + template + ".xml");
                        XmlElement templateRoot = tempalteDoc.DocumentElement;
                        FillExecuteWhen(templateRoot, ability);
                    }
                    else
                        FillExecuteWhen(xmlExecute, ability);
                }
   
                abilities.Add(ability);
              
                 
            }
                return abilities;
        }
        /// <summary>
        /// parse ExecuteWhen->Condition
        /// </summary>
        /// <param name="xmlExecute"></param>
        /// <param name="ability"></param>
        private static void FillExecuteWhen(XmlElement xmlExecute, Ability ability)
        {
            foreach (XmlElement xmlCondition in xmlExecute.SelectNodes("Condition"))
            {
                Condition condition = new Condition();
                condition.OrGroup = xmlCondition.Attributes.GetNamedItem("orgroup")?.Value.Trim();
                FillConditionChecks(xmlCondition, condition);

                ability.ExecuteWhen.Add(condition);
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

                if (String.Equals(xmlAttrib.Name,"type",StringComparison.OrdinalIgnoreCase) )           
                    check.CheckType = (CheckTypeEnm)System.Enum.Parse(typeof(CheckTypeEnm), xmlAttrib.Value.Trim());
                else if (String.Equals(xmlAttrib.Name, "value", StringComparison.OrdinalIgnoreCase))
                    check.Value =xmlAttrib.Value.Trim();
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
                unitDoc.Load(Utils.DataFolder + "UnitTemplates\\" + unitCode + ".xml");
                XmlElement xRoot = unitDoc.DocumentElement;
                if (xRoot != null)
                {
                    unit.Name = unitCode;
                    string elementVal = xRoot.Attributes.GetNamedItem("element")?.Value.Trim();
                    if (elementVal != null)
                        unit.Element = (Unit.ElementEnm)System.Enum.Parse(typeof(Unit.ElementEnm), elementVal);
                    unit.Stats = ExctractStats(xRoot);
                    unit.Abilities = ExctractAbilities(xRoot,unit);
                    units.Add(unit);
                }
                
            }
            return units;
        }

        /// <summary>
        /// Parsing XML part of Fights
        /// </summary>
        /// <param name="xnode">xml segment</param>
        private static void FillFights(XmlElement xnode,SimCls Combat)
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

