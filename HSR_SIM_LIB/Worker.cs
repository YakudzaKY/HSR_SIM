using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Permissions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static HSR_SIM_LIB.CallBacks;
using System.Runtime.Remoting.Contexts;
using System.Xml;
using System.Xml.Linq;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Main lib class
    /// </summary>
    public class Worker
    {


        CallBackStr cbLog;
        public CallBackStr CbLog { get => cbLog; set => cbLog = value; }
        public Combat Combat { get; set; } = null;


        /// <summary>
        /// Load and parse xml file with scenario
        /// </summary>
        /// <param name="selectedPath">file path to file</param>
        public void LoadScenarioFromXml(string selectedPath)
        {
            Combat = new Combat();
            Combat.Scenario= new Scenario();

            Combat.Scenario.Fights=new List<Fight> (); 
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(selectedPath);
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                Combat.Scenario.Name = xRoot.Attributes.GetNamedItem("name").Value;

                //parse all items
                foreach (XmlElement xnode in xRoot)
                {
                    if (xnode.Name =="Fights")
                    {
                        FillFights(xnode);
                    }
                    if (xnode.Name == "Party")
                    {
                        Combat.Scenario.Party = ExtractUnits(xnode);
                    }
                }

                LogText("Scenario name: " + Combat.Scenario.Name + " loaded");
            }


        }

        /// <summary>
        /// Exctract unit list from xml elemenet
        /// </summary>
        /// <param name="wave"></param>
        /// <returns></returns>
        private List<Unit> ExtractUnits(XmlElement unitPack)
        {
            List<Unit> units = new List<Unit>();
            foreach (XmlElement unitNode in unitPack.SelectNodes("Unit"))
            {
                Unit unit = new Unit();
                unit.Name = "TEST";
                /*
                 *TODO:  надо делать загрузку имени и первоначальных статов из шаблона
                unit.Name = unitNode.Attributes.GetNamedItem("name").Value;
                foreach (XmlElement stat in unitNode.SelectNodes("Stats"))
                {
                  
                }
                */
                units.Add(unit);
            }
            return units;
        }
        /// <summary>
        /// Parsing XML part of Fights
        /// </summary>
        /// <param name="xnode">xml segment</param>
        private void FillFights(XmlElement xnode)
        {
       

            foreach (XmlElement fightXml in xnode.ChildNodes)
            {
                Fight fg= new Fight();
                fg.Name= fightXml.Attributes.GetNamedItem("name").Value.Trim();
                fg.Waves = new List<Wave>();
                
                foreach (XmlElement waveXml in fightXml.SelectNodes("Wave"))
                {
                    Wave ww = new Wave();
                    ww.Units = ExtractUnits(waveXml);

                    fg.Waves.Add(ww);
                }
                Combat.Scenario.Fights.Add(fg);
            }
        }

        /// <summary>
        /// wrapper for Text callback using for log output
        /// </summary>
        /// <param name="msg">message to print</param>
        private void LogText(string msg)
        {
            if (CbLog!= null)
                CbLog  (new KeyValuePair<string, string>(Constant.MsgLog, msg));
        }


        public  Worker()
        {            
         

        }

        public void Init()
        {
            LogText("lib loaded");
        }
    }
}
