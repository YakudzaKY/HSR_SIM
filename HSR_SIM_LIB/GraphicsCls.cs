using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static HSR_SIM_LIB.Constant;
using static HSR_SIM_LIB.Resource;
using static HSR_SIM_LIB.Unit;
using static HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB
{/// <summary>
/// some drawing shit
/// </summary>
    public static class GraphicsCls
    {
        private enum DrawDirection
        {
            BottomToTop,
            TopToBottom
        }
        /// <summary>
        /// Render current situation in combat
        /// </summary>
        public static Bitmap RenderCombat(SimCls sim, bool replay)
        {

            Bitmap res = new(CombatImgSize.Width, CombatImgSize.Height);
            if (sim != null)
            {
                //background
                using Graphics gfx = Graphics.FromImage(res);
                using SolidBrush brush = new(Color.FromArgb(50, 50, 50));
                gfx.FillRectangle(brush, 0, 0, res.Width, res.Height);

                //party draw
                DrawUnits(gfx, sim.PartyTeam.Units, new Point(LeftSideWithSpace, BottomSideWithSpace), sim.CurrentStep,DrawDirection.BottomToTop);

                //TP/SP draw
                if (sim.CurrentFight != null)
                    DrawText(PartyResourceX, PartyResourceY, gfx, String.Format("SP: {0:d}/{1:d}", (int) sim.PartyTeam.GetRes(ResourceType.SP).ResVal, Constant.MaxSp));
                else
                    DrawText(PartyResourceX, PartyResourceY, gfx, String.Format("TP: {0:d}/{1:d}", (int) sim.PartyTeam.GetRes(ResourceType.TP).ResVal, Constant.MaxTp));

                //enemy draw
                if (sim.CurrentFight != null && sim.CurrentFight.CurrentWave!=null)
                {
                    DrawUnits(gfx, sim.HostileTeam.Units, 
                        new Point(LeftSideWithSpace, TopSideWithSpace), sim.CurrentStep,DrawDirection.TopToBottom);
                   
                }
                //Forgotten hall
                DrawUnits(gfx,  sim.SpecialTeam.Units, new Point((int)(CombatImgSize.Width / 1.35), CombatImgSize.Height / 2), sim.CurrentStep,null);

                if (sim.CurrentFight is null && sim.NextFight != null)
                {
                    //enemies of first Party unit
                    DrawNextHostile(gfx, sim.PartyTeam.Units.First().Enemies, new Point((CombatImgSize.Width / 2) - 3 * PortraitSizeMini.Width, TopSideWithSpace));

                }
                DrawStartQueue(gfx, new Point(LeftSideWithSpace, TopSideForQueue), sim.BeforeStartQueue);
                //step
                DrawCenterText(gfx, sim.CurrentStep.GetDescription());
                //events
                short i = 2;
                foreach (Event ent in sim.CurrentStep.Events)
                {
                    DrawText((int)(CombatImgSize.Width / 3.5) , CenterTextY+(i* DefaultFontSize), gfx, ent.GetDescription(), new SolidBrush(clrDefault), new("Tahoma", (int)(DefaultFontSize * 0.6), FontStyle.Bold));
                    i++;
                }
                DrawText(PartyResourceX, PartyResourceY - 3 * (int)(DefaultFontSize * 1.2), gfx, String.Format("Fight: {0:d}/{1:d}",
                        sim.CurrentFightStep, sim.CurrentScenario.Fights.Count));
                if (sim.CurrentFight != null)
                    DrawText(PartyResourceX, PartyResourceY - 2 * (int)(DefaultFontSize * 1.2), gfx, String.Format("Wave: {0:d}/{1:d}",
                        sim.CurrentFight.CurrentWaveCnt, sim.CurrentFight.ReferenceFight.Waves.Count));
                //if replay step. then need watermark
                if (replay)
                {
                    Bitmap bitmap = LoadBitmap("replay");
                    gfx.DrawImage(LoadBitmap("replay"), new Point(CombatImgSize.Width - bitmap.Width, CombatImgSize.Height - bitmap.Height));
                }


                gfx.Dispose();
                brush.Dispose();
            }
            return res;
        }

        /// <summary>
        /// Draw units and weakness from next fight
        /// </summary>
        /// <param name="gfx"></param>
        /// <param name="hostileParty"></param>
        /// <param name="point"></param>
        private static void DrawNextHostile(Graphics gfx, List<Unit> hostileParty, Point point)
        {
            short i = 0;
            List<ElementEnm> elemList = new ();
            foreach (Unit unit in hostileParty)
            {
                //portrait
                gfx.DrawImage(new Bitmap(unit.Portrait, PortraitSizeMini), new Point(point.X + (i * (int)(PortraitSizeMini.Width * 1.1)), point.Y));
                i++;
                if (i >= 3 && i < hostileParty.Count)
                {
                    gfx.DrawImage(new Bitmap(LoadBitmap("next"), PortraitSizeMini), new Point(point.X + (i * (int)(PortraitSizeMini.Width * 1.1)), point.Y));
                    break;
                }
                foreach (ElementEnm elm in unit.Fighter.Weaknesses)
                {
                    if (elemList.IndexOf(elm) < 0)
                        elemList.Add(elm);
                }
            }

            //Weakness
            i = 0;
            foreach (ElementEnm elm in elemList)
            {
                gfx.DrawImage(new Bitmap(LoadBitmap(elm.ToString()), ElemSizeMini),
                    new Point(point.X + (i * (int)(ElemSizeMini.Width * 1.1)), point.Y + (int)(PortraitSizeMini.Height * 1.1)));
                i++;
            }

            elemList.Clear();
        }

        /// <summary>
        /// Draw start skills queue(technique etc)
        /// </summary>
        /// <param name="gfx"></param>
        /// <param name="units"></param>
        /// <param name="hstl"></param>
        /// <param name="point"></param>
        private static void DrawStartQueue(Graphics gfx, Point point, List<Ability> startQueue)
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
                    new SolidBrush(clrGreen), new Font("Tahoma", StartQueuefontSize));
                i++;
            }
        }

        /// <summary>
        /// Convet to black and white
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Bitmap ConvertBlackAndWhite(Bitmap original)

        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new(original.Width, original.Height);
            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);
            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new (
               new float[][]
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });
            //create some image attributes
            ImageAttributes attributes = new();
            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);
            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }


        /// <summary>
        /// draw units in row
        /// </summary>
        /// <param name="gfx"> graphics</param>
        /// <param name="units"> Unit list</param>
        /// <param name="point"> start point</param>
        /// <param name="step"></param>
        /// <param name="topToBottom"></param>
        /// <param name="hstl">hostile type</param>
        private static void DrawUnits(Graphics gfx, List<Unit> units, Point point, Step step, DrawDirection? drawDirection)
        {
            short i = 0;

            foreach (Unit unit in units)
            {
                Point portraitPoint = new(point.X + (i * (UnitSpaceSize + TotalUnitSize.Width)), point.Y);
                //portrait
                if (unit.IsAlive)
                    gfx.DrawImage(unit.Portrait, portraitPoint);
                else
                {
                    //gray filter if unit is dead
                    Bitmap grayPortrait = ConvertBlackAndWhite(unit.Portrait);
                    gfx.DrawImage(grayPortrait, portraitPoint);
                    grayPortrait.Dispose();
                }
                //defeat flag
                if (step.Events.Any(x => x.Type == Event.EventType.Defeat && x.TargetUnit == unit))
                {
                    Bitmap btm = new Bitmap(LoadBitmap("defeat"), PortraitSize);
                    gfx.DrawImage(btm, portraitPoint);
                }
                //name
                DrawText(portraitPoint.X + 3, portraitPoint.Y + 3, gfx, String.Format("{0:s}({1:d})", unit.Name, unit.Level), null, new Font("Tahoma", 12, FontStyle.Bold),true);

                //elements
                gfx.DrawImage(new Bitmap(LoadBitmap(unit.Fighter.Element.ToString()), ElemSizeMini), new Point(portraitPoint.X + PortraitSize.Width - ElemSizeMini.Width, portraitPoint.Y));
                //weaknesses
                short j = 0;
                if (unit.Fighter.Weaknesses!=null)
                    foreach (ElementEnm weak in unit.Fighter.Weaknesses)
                    {
                        gfx.DrawImage(new Bitmap(LoadBitmap(weak.ToString()), ElemSizeMini), new Point(portraitPoint.X + j * ElemSizeMini.Width, portraitPoint.Y + (int)(PortraitSize.Height * 0.8)));
                        j++;
                    }

                //healthbar
                if (unit.Stats.MaxHp > 0)
                {
                    using (SolidBrush brush = new(Color.FromArgb(170, 000, 000)))
                    {
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height,
                            HealthBarSize.Width, HealthBarSize.Height);
                    }

                    using (SolidBrush brush = new(clrGreen))
                    {
                        int greenWidth =
                            (int)Math.Ceiling((double)HealthBarSize.Width * ((double)unit.GetRes(ResourceType.HP).ResVal) / unit.Stats.MaxHp);
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height, greenWidth,
                            HealthBarSize.Height);
                    }

                    DrawText(portraitPoint.X + 5, portraitPoint.Y + PortraitSize.Height, gfx,
                        String.Format("{0:d}/{1:d}",(int)Math.Ceiling((double)unit.GetRes(ResourceType.HP).ResVal), (int)Math.Ceiling(unit.Stats.MaxHp)), null,
                        new Font("Tahoma", BarFontSize));
                }

                //Energy bar
                if (unit.Stats.BaseMaxEnergy > 0)
                {

                    using (SolidBrush brush = new(Color.FromArgb(13, 26, 43)))
                    {
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, EnergyBarSize.Width, EnergyBarSize.Height);
                    }
                    using (SolidBrush brush = new(Color.FromArgb(43, 83, 140)))
                    {
                        int blueWidth = (int)Math.Round((double)EnergyBarSize.Width * (unit.Stats.CurrentEnergy) / unit.Stats.BaseMaxEnergy);
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, blueWidth, EnergyBarSize.Height);
                    }
                    DrawText(portraitPoint.X + 5
                        , portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height
                        , gfx
                        , String.Format("{0:d}/{1:d}", unit.Stats.CurrentEnergy, unit.Stats.BaseMaxEnergy)
                        , null
                        , new Font("Tahoma", BarFontSize));
                }

                //Toughness
                if (unit.Stats.MaxToughness > 0)
                {

                    using (SolidBrush brush = new(Color.FromArgb(61, 61, 61)))
                    {
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, EnergyBarSize.Width, EnergyBarSize.Height);
                    }
                    using (SolidBrush brush = new(Color.FromArgb(182, 182, 182)))
                    {
                        int blueWidth = (int)Math.Round((double)EnergyBarSize.Width * ((double)unit.GetRes(ResourceType.Toughness).ResVal) / unit.Stats.MaxToughness);
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, blueWidth, EnergyBarSize.Height);
                    }
                    DrawText(portraitPoint.X + 5
                        , portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height
                        , gfx
                        , String.Format("{0:d}/{1:d}", (int) unit.GetRes(ResourceType.Toughness).ResVal, unit.Stats.MaxToughness)
                        , null
                        , new Font("Tahoma", BarFontSize));
                }
                //If unit is actor
                if (step.Actor == unit)
                {
                    gfx.DrawRectangle(new Pen(Color.YellowGreen, 3), portraitPoint.X, portraitPoint.Y, PortraitSize.Width, PortraitSize.Height);
                }
                //if target
                if (step.Events.Any(x => x.TargetUnit == unit||(x.Type==Event.EventType.Mod&&x.Mods.Any(y=>y.TargetUnit==unit))))
                {
                    gfx.DrawRectangle(new Pen(Color.BurlyWood, 3), portraitPoint.X+(int)(PortraitSize.Width*0.05), portraitPoint.Y+(int)(PortraitSize.Height*0.05), PortraitSize.Width-(int)(PortraitSize.Width*0.1), PortraitSize.Height-(int)(PortraitSize.Width*0.1));
                }

                //dmg got
                if (drawDirection != null)
                {
                    j = 0;
                    foreach (Event ent in step.Events.Where(x=>x.TargetUnit==unit &&
                                                               (x.Type==Event.EventType.DirectDamage
                                                               ||x.Type==Event.EventType.ShieldBreak
                                                               ||x.Type==Event.EventType.DoTDamage
                                                               ||(x.Type==Event.EventType.ResourceDrain&&x.Val>0&&x.ResType==ResourceType.HP))
                                                               ))
                    {
                        int pointY = 0;
                        if (drawDirection == DrawDirection.TopToBottom)
                            pointY = portraitPoint.Y + TotalUnitSize.Height + (CombatImgSize.Height / 50 * j);
                        else
                        {
                            pointY = portraitPoint.Y  - (CombatImgSize.Height / 50 * (j+2));
                        }

                        DrawText(portraitPoint.X 
                            , pointY
                            , gfx
                            ,  Math.Ceiling((double)(ent.Val??0)).ToString()
                            , new SolidBrush( Color.Red)
                            , new Font("Tahoma",  BarFontSize));

                        j++;
                    }
                }

                //Buffs
                j = 0;
                foreach (var buff in unit.Mods)
                {
                    var buffPoint = new Point(portraitPoint.X + PortraitSize.Width,
                        portraitPoint.Y + j * ElemSizeMini.Height);
                    gfx.DrawImage(new Bitmap(LoadBitmap(buff.Modifier.ToString()), ElemSizeMini), buffPoint);
                    gfx.DrawRectangle(new Pen((buff.Type == Mod.ModType.Buff) ? Color.Aquamarine : Color.Brown, 1), buffPoint.X, buffPoint.Y, ElemSizeMini.Width, ElemSizeMini.Height);

                    j++;
                }


               
                if (step.Parent.CurrentFight?.CurrentWave != null)
                {
                    //AV
                    DrawText(portraitPoint.X + 5, portraitPoint.Y + (int)(PortraitSize.Height * 0.4), gfx, Math.Floor(unit.Stats.ActionValue).ToString(), new SolidBrush(Color.Violet), new Font("Tahoma", DefaultFontSize),true);
                }

                if (step.Parent.CurrentFight!=null)
                    //Special text
                    DrawText(portraitPoint.X + 5, portraitPoint.Y + (int)(PortraitSize.Height * 0.6), gfx, unit.Fighter.GetSpecialText(), new SolidBrush(Color.Chartreuse), new Font("Tahoma", DefaultFontSize-2),true);

                i++;
            }
        }
        /// <summary>
        /// text in the middle
        /// </summary>
        private static void DrawCenterText(Graphics gfx, string text, Brush brush = null)
        {
            DrawText((CombatImgSize.Width / 2) - (text.Length * DefaultFontSizeSpace), CenterTextY, gfx, text, brush);
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
        private static void DrawText(int x, int y, Graphics gfx, string text, Brush brush = null, Font font = null,bool drawBack=false)
        {
            if (string.IsNullOrEmpty(text))
                return;

            brush ??= new SolidBrush(clrDefault);
            font ??= new("Tahoma", DefaultFontSize, FontStyle.Bold);

            if (drawBack )
                gfx.FillRectangle(new SolidBrush( Color.FromArgb(155,  50, 50, 50)), x+3, y,(int)Math.Round((double)text.Length*(double)font.Size*0.7), (int)Math.Round(font.Size*1.6));
            gfx.DrawString(text, font, brush, new Point(x,y));
            


        }


    }
}
