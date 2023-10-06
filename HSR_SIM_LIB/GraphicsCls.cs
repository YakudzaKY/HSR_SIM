using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HSR_SIM_LIB.Constant;
using System.Drawing;

namespace HSR_SIM_LIB
{/// <summary>
/// some drawing shit
/// </summary>
    public static class GraphicsCls
    {
        /// <summary>
        /// Render current situation in combat
        /// </summary>
        public static Bitmap RenderCombat(CombatCls Combat)
        {

            Bitmap res = new Bitmap(CombatImgSize.Width, CombatImgSize.Height);
            if (Combat != null)
            {
                //background
                using (Graphics gfx = Graphics.FromImage(res))
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(155, 155, 155)))
                    {
                        gfx.FillRectangle(brush, 0, 0, res.Width, res.Height);
                    }
                    //party draw
                    DrawUnits(gfx, Combat.Party, unitHostility.Friendly, new Point(10, CombatImgSize.Height - TotalUnitSize.Height - 10));
                    //TP draw
                    DrawText(CombatImgSize.Width - 100, CombatImgSize.Height / 2, gfx, String.Format("TP: {0:d}/{1:d}", Combat.Tp, Constant.MaxTp));
                    //enemy draw
                    if (Combat.CurrentFight != null)
                    {
                        DrawUnits(gfx, Combat.CurrentFight.Units, unitHostility.Hostile, new Point(10, 10));
                        DrawText(CombatImgSize.Width - 100, CombatImgSize.Height / 2 - 15, gfx, String.Format("Fight: {0:d}/{1:d}",
                            Combat.CurrentScenario.Fights.IndexOf(Combat.CurrentFight), Combat.CurrentScenario.Fights.Count));
                    }
                   

                    if (Combat.CurrentFight is null && Combat.NextFight != null)
                    {
                        DrawCenterText(gfx, "waiting for combat");
                    }
                    else if (Combat.CurrentFight is null && Combat.NextFight == null)
                    {
                        DrawCenterText(gfx, "scenario completed");
                    }
                }
            }
            return res;
        }
   

    /// <summary>
    /// draw units in row
    /// </summary>
    /// <param name="gfx"> graphics</param>
    /// <param name="units"> Unit list</param>
    /// <param name="hstl">hostile type</param>
    /// <param name="point"> start point</param>
    private static void DrawUnits(Graphics gfx, List<Unit> units, unitHostility hstl, Point point)
    {
        short i = 0;
        int spaceXSize = (int)Math.Round((double)((CombatImgSize.Width - (5 * TotalUnitSize.Width)) / 5));
        foreach (Unit unit in units)
        {
            Point portraitPoint = new Point(point.X + (i * (spaceXSize + TotalUnitSize.Width)), point.Y);
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
                    gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, blueWidth, EnergyBarSize.Height);
                }
                DrawText(portraitPoint.X + 5, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, gfx, String.Format("{0:d}/{1:d}", unit.Stats.CurrentEnergy, unit.Stats.BaseMaxEnergy), null, new Font("Tahoma", 7));
            }


            i++;
        }
    }
    /// <summary>
    /// text in the middle
    /// </summary>
    private static void DrawCenterText(Graphics gfx, string text, Brush brush = null)
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
    private  static void DrawText(int x, int y, Graphics gfx, string text, Brush brush = null, Font font = null)
    {
        if (brush == null)
            brush = Brushes.Black;
        if (font == null)
            font = new Font("Tahoma", 14, FontStyle.Bold);
        RectangleF rectf = new RectangleF(x, y, CombatImgSize.Width - x, CombatImgSize.Height - y);
        gfx.DrawString(text, font, brush, rectf);
    }


    }
}
