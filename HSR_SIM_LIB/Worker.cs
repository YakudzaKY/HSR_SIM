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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Drawing;
using ImageMagick;
using static HSR_SIM_LIB.Constant;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Main lib class
    /// </summary>
    public class Worker
    {


        CallBackStr cbLog;
        public CallBackStr CbLog { get => cbLog; set => cbLog = value; }

        CallBackRender cbRend;
        public CallBackRender CbRend { get => cbRend; set => cbRend = value; }

        public Combat Combat { get; set; } = null;

        /// <summary>
        /// draw units in row
        /// </summary>
        /// <param name="gfx"> graphics</param>
        /// <param name="units"> Unit list</param>
        /// <param name="hstl">hostile type</param>
        /// <param name="point"> start point</param>
        public void DrawUnits(Graphics gfx,List<Unit> units, unitHostility hstl, Point point)
        {
            short i = 0;
            int spaceXSize = (int)Math.Round ((double)((CombatImgSize.Width - (5 * TotalUnitSize.Width) )/5)) ;
            foreach (Unit unit in units)
            {
                Point portraitPoint = new Point(point.X + (i * (spaceXSize + TotalUnitSize.Width)), point.Y);
                //portrait
                gfx.DrawImage(unit.Portrait, portraitPoint);
                //name
                DrawText(portraitPoint.X+3, portraitPoint.Y+3, gfx, unit.Name,null, new Font("Tahoma", 12, FontStyle.Bold));
                //healthbar
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(170, 000, 000)))
                {
                    gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y+ PortraitSize.Height, HealthBarSize.Width, HealthBarSize.Height);
                }
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(000, 170, 000)))
                {
                    int greenWidth =(int)Math.Floor((double)HealthBarSize.Width *(unit.Stats.CurrentHp) / unit.Stats.MaxHp);
                    gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height, greenWidth, HealthBarSize.Height);
                }
                DrawText(portraitPoint.X + 5, portraitPoint.Y + PortraitSize.Height, gfx, String.Format("{0:d}/{1:d}", unit.Stats.CurrentHp, unit.Stats.MaxHp), null, new Font("Tahoma", 7));
                //Energy bar
                if (unit.Stats.BaseMaxEnergy > 0)
                {
                    
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(252, 217, 167)))
                    {
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, EnergyBarSize.Width, EnergyBarSize.Height);
                    }
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(4, 232, 255)))
                    {
                        int blueWidth = (int)Math.Floor((double)EnergyBarSize.Width * (unit.Stats.CurrentEnergy) / unit.Stats.BaseMaxEnergy);
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height+ HealthBarSize.Height, blueWidth, EnergyBarSize.Height);
                    }
                    DrawText(portraitPoint.X + 5, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, gfx, String.Format("{0:d}/{1:d}", unit.Stats.CurrentEnergy, unit.Stats.BaseMaxEnergy), null, new Font("Tahoma", 7));
                }

               
                i++;
            }  
        }
        /// <summary>
        /// text in the middle
        /// </summary>
        public void DrawCenterText(Graphics gfx,string text, Brush brush = null)
        {
            DrawText((int)Math.Round(CombatImgSize.Width * 0.4), (int)Math.Round(CombatImgSize.Height * 0.4), gfx, text, brush);
        }

        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="gfx"></param>
        /// <param name="text"></param>
        /// <param name="brush"></param>
        /// <param name="font"></param>
        public void DrawText(int x, int y, Graphics gfx, string text, Brush brush = null, Font font =null)
        {
            if (brush == null)
                brush = Brushes.Black;
            if (font == null)
                font = new Font("Tahoma", 14, FontStyle.Bold);
            RectangleF rectf = new RectangleF(x, y, CombatImgSize.Width-x, CombatImgSize.Height-y);
            gfx.DrawString(text, font, brush, rectf);
        }

   /// <summary>
   /// Render current situation in combat
   /// </summary>
        public Bitmap RenderCombat()
        {

            Bitmap res = new Bitmap (CombatImgSize.Width, CombatImgSize.Height); 
            if (Combat!=null )
            {
                //background
                using (Graphics gfx = Graphics.FromImage(res))
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(155, 155, 155)))
                    {
                        gfx.FillRectangle(brush, 0, 0, res.Width, res.Height);
                    }
                    //party draw
                    DrawUnits(gfx, Combat.Party, unitHostility.Friendly, new Point(10, CombatImgSize.Height - TotalUnitSize.Height-10));
                    //TP draw
                    DrawText(CombatImgSize.Width - 100, CombatImgSize.Height / 2, gfx, String.Format("TP: {0:d}/{1:d}",Combat.Tp , Constant.MaxTp) );
                    //enemy draw
                    if (Combat.CurrentFight != null)
                    {
                        DrawUnits(gfx, Combat.CurrentFight.Units, unitHostility.Hostile, new Point(10, 10));
                    }
                    

                    if (Combat.CurrentFight is null && Combat.NextFight != null)
                    {
                        DrawCenterText(gfx, "waiting for combat");                     
                    }
                    else if (Combat.CurrentFight is null && Combat.NextFight== null)
                    {
                        DrawCenterText(gfx, "scenario completed");
                    }
                }               
            }
            return res;
        }

        /// <summary>
        /// Draw combat in client
        /// </summary>
        public void DrawCombat()
        { 
            if (CbRend != null)
                CbRend( RenderCombat());
        }
        /// <summary>
        /// Load and parse xml file with scenario
        /// </summary>
        /// <param name="selectedPath">file path to file</param>
        public void LoadScenarioFromXml(string selectedPath)
        {
            Combat = new Combat();
            Combat.CurrentScenario = new Scenario();

            Combat.CurrentScenario.Fights=new List<Fight> (); 
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(selectedPath);
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                Combat.CurrentScenario.Name = xRoot.Attributes.GetNamedItem("name").Value;

                //parse all items
                foreach (XmlElement xnode in xRoot)
                {
                    if (xnode.Name =="Fights")
                    {
                        FillFights(xnode);
                    }
                    if (xnode.Name == "Party")
                    {
                        Combat.CurrentScenario.Party = ExtractUnits(xnode);
                    }
                }

                LogText("Scenario name: " + Combat.CurrentScenario.Name + " loaded");
            }

            Combat.Prepare();
            DrawCombat();
            
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
                XmlDocument unitDoc = new XmlDocument();
                Unit unit= new Unit ();
                //load xml by 
                string unitCode = unitNode.Attributes.GetNamedItem("template").Value.Trim();
                unitDoc.Load(Utils.DataFolder+"UnitTemplates\\" + unitCode + ".xml");
                XmlElement xRoot = unitDoc.DocumentElement;
                if (xRoot != null)
                {
                    unit.Name = unitCode;
                    //parse all items
                    foreach (XmlElement xnode in xRoot)
                    {
                        if (xnode.Name == "Stats")
                        {
                            unit.Stats.BaseMaxHp = int.Parse(xnode.Attributes.GetNamedItem("maxHp").Value.Trim());
                            unit.Stats.BaseAttack = int.Parse(xnode.Attributes.GetNamedItem("attack").Value.Trim());
                            if (xnode.Attributes.GetNamedItem("energy") != null)
                                unit.Stats.BaseMaxEnergy = int.Parse(xnode.Attributes.GetNamedItem("energy").Value.Trim());
                            
                        }
                      
                    }

                    units.Add(unit);
                }              
                    
                   
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
                Combat.CurrentScenario.Fights.Add(fg);
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
