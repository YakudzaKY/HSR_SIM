using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HSR_SIM_LIB;

namespace HSR_SIM_LIB
{
    public static class XMLLoader
    {
        public static CombatCls LoadCombatFromXml(string ScenarioPath)
        {

            CombatCls Combat = new CombatCls();
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
        /// exctract one ability
        /// </summary>
        /// <param name="xmlAbility"></param>
        /// <returns></returns>
        private static Ability ExctractAbility(XmlElement xmlAbility)
        {
            Ability ability = new Ability();
            ability.AbilityParams.Name = xmlAbility.Attributes.GetNamedItem("name").Value.Trim();
            ability.AbilityParams.AbilityType  = (Ability.AbilityTypeEnm)System.Enum.Parse(typeof(Ability.AbilityTypeEnm), getXmlTagValue(xmlAbility, "Type") );
            ability.AbilityParams.EnterCombat = Convert.ToBoolean(getXmlTagValue(xmlAbility, "EnterCombat")) ;
            ability.AbilityParams.IgnoreWeaknes = Convert.ToBoolean(getXmlTagValue(xmlAbility, "IgnoreWeaknes"))  ;
            return ability;
        }
        /// <summary>
        /// Exctract abilities(is not mandatory in xml)
        /// </summary>
        /// <param name="xmlItems"></param>
        /// <returns></returns>
        private static List<Ability> ExctractAbilities(XmlElement xmlItems)
        {
            List<Ability> abilities = new List<Ability>();
            foreach (XmlElement abilitiyXml in xmlItems.SelectNodes("Ability"))
            {
               
                abilities.Add(ExctractAbility(abilitiyXml));
              
                 
            }
                return abilities;
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
                    unit.Stats = ExctractStats(xRoot);
                    unit.Abilities = ExctractAbilities(xRoot);
                    units.Add(unit);
                }


            }
            return units;
        }

        /// <summary>
        /// Parsing XML part of Fights
        /// </summary>
        /// <param name="xnode">xml segment</param>
        private static void FillFights(XmlElement xnode,CombatCls Combat)
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

