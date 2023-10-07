using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HSR_SIM_LIB.Constant;
using System.Drawing;
using static HSR_SIM_LIB.Resource;
using static HSR_SIM_LIB.Unit;

namespace HSR_SIM_LIB
{/// <summary>
/// some drawing shit
/// </summary>
    public static class GraphicsCls
    {
        /// <summary>
        /// Render current situation in combat
        /// </summary>
        public static Bitmap RenderCombat(SimCls sim)
        {

            Bitmap res = new Bitmap(CombatImgSize.Width, CombatImgSize.Height);
            if (sim != null)
            {
                //background
                using (Graphics gfx = Graphics.FromImage(res))
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(155, 155, 155)))
                    {
                        gfx.FillRectangle(brush, 0, 0, res.Width, res.Height);
                    }
                    //party draw
                    DrawUnits(gfx, sim.Party, unitHostility.Friendly, new Point(LeftSideWithSpace, BottomSideWithSpace), sim.CurrentStep);
                    //TP draw
                    DrawText(PartyResourceX, PartyResourceY, gfx, String.Format("TP: {0:d}/{1:d}", sim.GetRes(ResourceType.TP).ResVal, Constant.MaxTp));
                    //enemy draw
                    if (sim.CurrentFight != null)
                    {
                        DrawUnits(gfx, sim.CurrentFight.Units, unitHostility.Hostile, new Point(LeftSideWithSpace, TopSideWithSpace), sim.CurrentStep);
                        DrawText(PartyResourceX, PartyResourceY- DefaultFontSize, gfx, String.Format("Fight: {0:d}/{1:d}",
                            sim.CurrentScenario.Fights.IndexOf(sim.CurrentFight), sim.CurrentScenario.Fights.Count));
                    }
                   

                    if (sim.CurrentFight is null && sim.NextFight != null)
                    {
                        DrawStartQueue(gfx, new Point(LeftSideWithSpace, TopSideForQueue), sim.BeforeStartQueue);
                    }

                    //TODO: draw next fight units (with weaknes)
                    DrawCenterText(gfx, sim.CurrentStep.GetStepDescription());
                }
            }
            return res;
        }
        /// <summary>
        /// Draw start skills queue(technique etc)
        /// </summary>
        /// <param name="gfx"></param>
        /// <param name="units"></param>
        /// <param name="hstl"></param>
        /// <param name="point"></param>
        private static void DrawStartQueue(Graphics gfx,Point point,List<Ability> startQueue)
        {
          
          
            short i = 0;
            if (startQueue.Count > 0)
            {
                DrawText(point.X, point.Y + ((StartQueuefontSize + StartQueuefontSizeSpc) * i), gfx, "Start skills queue:", null, new Font("Tahoma", StartQueuefontSize));
                i++;
            }
            
            foreach (Ability ability in startQueue)
            {
                DrawText(point.X, point.Y + ((StartQueuefontSize + StartQueuefontSizeSpc) * i), gfx, String.Format("{0:s}: {1:s}", ability.Parent.Name, ability.Name),
                    Brushes.Lime, new Font("Tahoma", StartQueuefontSize));              
                i++;
            }
        }
        /// <summary>
        /// draw units in row
        /// </summary>
        /// <param name="gfx"> graphics</param>
        /// <param name="units"> Unit list</param>
        /// <param name="hstl">hostile type</param>
        /// <param name="point"> start point</param>
        private static void DrawUnits(Graphics gfx, List<Unit> units, unitHostility hstl, Point point, Step step)
    {
        short i = 0;
       
        foreach (Unit unit in units)
        {
            Point portraitPoint = new Point(point.X + (i * (UnitSpaceSize + TotalUnitSize.Width)), point.Y);
            //portrait
            gfx.DrawImage(unit.Portrait, portraitPoint);
            //name
            DrawText(portraitPoint.X + 3, portraitPoint.Y + 3, gfx, unit.Name, null, new Font("Tahoma", 12, FontStyle.Bold));
            //healthbar
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(170, 000, 000)))
            {
                gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height, HealthBarSize.Width, HealthBarSize.Height);
            }
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(000, 170, 000)))
            {
                int greenWidth = (int)Math.Floor((double)HealthBarSize.Width * (unit.Stats.CurrentHp) / unit.Stats.MaxHp);
                gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height, greenWidth, HealthBarSize.Height);
            }
            DrawText(portraitPoint.X + 5, portraitPoint.Y + PortraitSize.Height, gfx, String.Format("{0:d}/{1:d}", unit.Stats.CurrentHp, unit.Stats.MaxHp), null, new Font("Tahoma", BarFontSize));
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
                    gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, blueWidth, EnergyBarSize.Height);
                }
                DrawText(portraitPoint.X + 5
                    , portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height
                    , gfx
                    , String.Format("{0:d}/{1:d}", unit.Stats.CurrentEnergy, unit.Stats.BaseMaxEnergy)
                    , null
                    , new Font("Tahoma", BarFontSize));
            }

            //If unit is actor
            if (step.Actor == unit)
            {                    
                    gfx.DrawRectangle(new Pen(Color.YellowGreen, 3), portraitPoint.X, portraitPoint.Y, PortraitSize.Width, PortraitSize.Height);
            }


            i++;
        }
    }
    /// <summary>
    /// text in the middle
    /// </summary>
    private static void DrawCenterText(Graphics gfx, string text, Brush brush = null)
    {
        DrawText(CombatImgSize.Width-(text.Length* DefaultFontSizeSpace), CenterTextY, gfx, text, brush);
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
    private  static void DrawText(int x, int y, Graphics gfx, string text, Brush brush = null, Font font = null)
    {
        if (brush == null)
            brush = Brushes.Black;
        if (font == null)
            font = new Font("Tahoma", DefaultFontSize, FontStyle.Bold);
        RectangleF rectf = new RectangleF(x, y, CombatImgSize.Width - x, CombatImgSize.Height - y);
        gfx.DrawString(text, font, brush, rectf);
    }


    }
}
