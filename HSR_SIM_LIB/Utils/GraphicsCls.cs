using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using static System.Net.Mime.MediaTypeNames;
using static HSR_SIM_LIB.Utils.Constant;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.UnitStuff.Unit;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Utils
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
                DrawUnits(gfx, sim.PartyTeam.Units, new Point(LeftSideWithSpace, BottomSideWithSpace), sim.CurrentStep, DrawDirection.BottomToTop);

                //TP/SP draw
                if (sim.CurrentFight != null)
                    DrawText(PartyResourceX, PartyResourceY, gfx,
                        $"SP: {(int)sim.PartyTeam.GetRes(ResourceType.SP).ResVal:d}/{MaxSp:d}");
                else
                    DrawText(PartyResourceX, PartyResourceY, gfx,
                        $"TP: {(int)sim.PartyTeam.GetRes(ResourceType.TP).ResVal:d}/{MaxTp:d}");

                //enemy draw
                if (sim.CurrentFight is { CurrentWave: not null })
                {
                    DrawUnits(gfx, sim.HostileTeam.Units,
                        new Point(LeftSideWithSpace, TopSideWithSpace), sim.CurrentStep, DrawDirection.TopToBottom);

                }
                //Forgotten hall
                DrawUnits(gfx, sim.SpecialTeam.Units, new Point((int)(CombatImgSize.Width / 1.35), CombatImgSize.Height / 2), sim.CurrentStep, null);

                if (sim.CurrentFight is null && sim.NextFight != null)
                {
                    //enemies of first Party unit
                    DrawNextHostile(gfx, sim.PartyTeam.Units.First().Enemies, new Point(CombatImgSize.Width / 2 - 3 * PortraitSizeMini.Width, TopSideWithSpace));

                }
                DrawStartQueue(gfx, new Point(LeftSideWithSpace, TopSideForQueue), sim.BeforeStartQueue);
                //step
                DrawCenterText(gfx, sim.CurrentStep.GetDescription());
                DrawText(PartyResourceX, PartyResourceY - 3 * (int)(DefaultFontSize * 1.2), gfx, string.Format("Fight: {0:d}/{1:d}",
                        sim.CurrentFightStep, sim.CurrentScenario.Fights.Count));
                if (sim.CurrentFight != null)
                    DrawText(PartyResourceX, PartyResourceY - 2 * (int)(DefaultFontSize * 1.2), gfx, string.Format("Wave: {0:d}/{1:d}",
                        sim.CurrentFight.CurrentWaveCnt, sim.CurrentFight.ReferenceFight.Waves.Count));
                //if replay step. then need watermark
                if (replay)
                {
                    Bitmap bitmap = Utl.LoadBitmap("replay");
                    gfx.DrawImage(Utl.LoadBitmap("replay"), new Point(CombatImgSize.Width - bitmap.Width, CombatImgSize.Height - bitmap.Height));
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
            List<ElementEnm> elemList = new();
            foreach (Unit unit in hostileParty)
            {
                //portrait
                gfx.DrawImage(new Bitmap(unit.Portrait, PortraitSizeMini), new Point(point.X + i * (int)(PortraitSizeMini.Width * 1.1), point.Y));
                i++;
                if (i >= 3 && i < hostileParty.Count)
                {
                    gfx.DrawImage(new Bitmap(Utl.LoadBitmap("next"), PortraitSizeMini), new Point(point.X + i * (int)(PortraitSizeMini.Width * 1.1), point.Y));
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
                gfx.DrawImage(new Bitmap(Utl.LoadBitmap(elm.ToString()), ElemSizeMini),
                    new Point(point.X + i * (int)(ElemSizeMini.Width * 1.1), point.Y + (int)(PortraitSizeMini.Height * 1.1)));
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
                DrawText(point.X, point.Y + (StartQueuefontSize + StartQueuefontSizeSpc) * i, gfx, "Start skills queue:", null, new Font("Tahoma", StartQueuefontSize));
                i++;
            }

            foreach (Ability ability in startQueue)
            {
                DrawText(point.X, point.Y + (StartQueuefontSize + StartQueuefontSizeSpc) * i, gfx, string.Format("{0:s}: {1:s}", ability.Parent.Parent.Name, ability.Name),
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
            ColorMatrix colorMatrix = new(
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
        public static Bitmap ResizeBitmap(Bitmap b, int nWidth, int nHeight)
        {
            Bitmap result = new(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage(result))
            {
                // g.InterpolationMode = InterpolationMode.NearestNeighbor;
                double scMode = (double)nWidth / (double)b.Width;
                g.DrawImage(b, 0, 0, (int)Math.Round(b.Width * scMode), (int)Math.Round(b.Height * scMode));
            }
            return result;
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
                Point portraitPoint = new(point.X + i * (UnitSpaceSize + TotalUnitSize.Width), point.Y);
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
                if (step.Events.Any(x => x is Defeat && x.TargetUnit == unit))
                {
                    Bitmap btm = new(Utl.LoadBitmap("defeat"), PortraitSize);
                    gfx.DrawImage(btm, portraitPoint);
                }
                if (unit.Controlled)
                    gfx.DrawImage(new Bitmap( Utl.LoadBitmap("CC"), PortraitSize), portraitPoint);
                //Role
                FighterUtils.UnitRole? unitRole = unit.Fighter.Role;
                if (unitRole != null)
                {
                    gfx.DrawImage(new Bitmap(Utl.LoadBitmap("role" + unitRole.ToString()), ElemSizeMini), new Point(portraitPoint.X + PortraitSize.Width - ElemSizeMini.Width, portraitPoint.Y + PortraitSize.Height - ElemSizeMini.Height));
                }

                //name
                DrawText(portraitPoint.X + 3, portraitPoint.Y + 3, gfx, string.Format("{0:s}({1:d})", unit.Name, unit.Level), null, new Font("Tahoma", txtNameSize, FontStyle.Bold), true);
                //aggro
                if (unit.GetAggro(null) > 0)
                    DrawText(portraitPoint.X + 3, portraitPoint.Y + txtNameSize * 2, gfx, $"aggro: {(int)Math.Round(unit.GetAggro(null)):d} ({(int)Math.Round((unit.GetAggro(null) / unit.ParentTeam.TeamAggro) * 100):d}%)", new SolidBrush(Color.Coral), new Font("Tahoma", BarFontSize, FontStyle.Bold), true);
                //elements
                gfx.DrawImage(new Bitmap(Utl.LoadBitmap(unit.Fighter.Element.ToString()), ElemSizeMini), new Point(portraitPoint.X + PortraitSize.Width - ElemSizeMini.Width, portraitPoint.Y));
                //weaknesses
                short j = 0;
                if (unit.Fighter.Weaknesses != null)
                    foreach (ElementEnm weak in unit.Fighter.Weaknesses)
                    {
                        gfx.DrawImage(new Bitmap(Utl.LoadBitmap(weak.ToString()), ElemSizeMini), new Point(portraitPoint.X + j * ElemSizeMini.Width, portraitPoint.Y + (int)(PortraitSize.Height * 0.8)));
                        j++;
                    }

                //healthbar
                if (unit.GetMaxHp(null) > 0)
                {
                    using (SolidBrush brush = new(Color.FromArgb(170, 000, 000)))
                    {
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height,
                            HealthBarSize.Width, HealthBarSize.Height);
                    }

                    using (SolidBrush brush = new(clrGreen))
                    {
                        int greenWidth =
                            (int)Math.Ceiling(HealthBarSize.Width * (double)unit.GetRes(ResourceType.HP).ResVal / unit.GetMaxHp(null));
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height, greenWidth,
                            HealthBarSize.Height);
                    }

                    DrawText(portraitPoint.X + 5, portraitPoint.Y + PortraitSize.Height, gfx,
                        string.Format("{0:d}/{1:d}", (int)Math.Floor((double)unit.GetRes(ResourceType.HP).ResVal), (int)Math.Floor(unit.GetMaxHp(null))), null,
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
                        int blueWidth = (int)Math.Round((double)EnergyBarSize.Width * unit.CurrentEnergy / unit.Stats.BaseMaxEnergy);
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, blueWidth, EnergyBarSize.Height);
                    }
                    DrawText(portraitPoint.X + 5
                        , portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height
                        , gfx
                        , string.Format("{0:d}/{1:d}", (int)Math.Floor(unit.CurrentEnergy), (int)Math.Floor(unit.Stats.BaseMaxEnergy))
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
                        int blueWidth = (int)Math.Round(EnergyBarSize.Width * (double)unit.GetRes(ResourceType.Toughness).ResVal / unit.Stats.MaxToughness);
                        gfx.FillRectangle(brush, portraitPoint.X, portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height, blueWidth, EnergyBarSize.Height);
                    }
                    DrawText(portraitPoint.X + 5
                        , portraitPoint.Y + PortraitSize.Height + HealthBarSize.Height
                        , gfx
                        , string.Format("{0:d}/{1:d}", (int)unit.GetRes(ResourceType.Toughness).ResVal, unit.Stats.MaxToughness)
                        , null
                        , new Font("Tahoma", BarFontSize));
                }
                //If unit is actor
                if (step.Actor == unit)
                {
                    gfx.DrawRectangle(new Pen(Color.YellowGreen, 3), portraitPoint.X, portraitPoint.Y, PortraitSize.Width, PortraitSize.Height);
                }
                //if target
                if (step.Events.Any(x => x.TargetUnit == unit))
                {
                    gfx.DrawRectangle(new Pen(Color.BurlyWood, 3), portraitPoint.X + (int)(PortraitSize.Width * 0.05), portraitPoint.Y + (int)(PortraitSize.Height * 0.05), PortraitSize.Width - (int)(PortraitSize.Width * 0.1), PortraitSize.Height - (int)(PortraitSize.Width * 0.1));
                }

                //dmg got
                if (drawDirection != null)
                {
                    j = 0;
                    foreach (Event ent in step.Events.Where(x => x.TargetUnit == unit &&
                                                               (x.IsDamageEvent
                                                               || x is Healing
                                                               || (x is ResourceDrain && x.Val > 0 && ((ResourceDrain)x).ResType is ResourceType.HP or ResourceType.Toughness)
                                                               || (x is ResourceGain && x.Val > 0 && ((ResourceGain)x).ResType == ResourceType.HP)
                                                               )))
                    {
                        int pointY = 0;
                        if (drawDirection == DrawDirection.TopToBottom)
                            pointY = portraitPoint.Y + TotalUnitSize.Height + CombatImgSize.Height / 50 * j;
                        else
                        {
                            pointY = portraitPoint.Y - CombatImgSize.Height / 50 * (j + 2);
                        }
                        Bitmap dmgIcon = ent  switch
                        {
                            ToughnessBreak=> Utl.LoadBitmap("BreakShield"),
                            ToughnessBreakDoTDamage =>  Utl.LoadBitmap("BreakShieldDoT"),
                            DoTDamage =>  Utl.LoadBitmap("DoT"),
                            DirectDamage => Utl.LoadBitmap("Sword"),
                            ResourceDrain rdr => (rdr.ResType==ResourceType.HP)?Utl.LoadBitmap("Blood"):Utl.LoadBitmap("Scratch"),
                            ResourceGain =>Utl.LoadBitmap("Blood"),
                            Healing =>Utl.LoadBitmap("Healing"),
                            _ => null
                        };

                        if (dmgIcon!=null)
                        {
                            gfx.DrawImage(new Bitmap(dmgIcon, DmgIconSize), new Point(portraitPoint.X,pointY+1));
                        }

                        Color nmbrColor;
                        if (ent.IsDamageEvent)
                        {
                            nmbrColor = Unit.GetColorByElem(ent.AbilityValue?.Element);
                        }
                        else if (ent is ResourceGain or Healing)
                        {
                            nmbrColor = Color.GreenYellow;
                        }
                        else if (ent is ResourceDrain rdr)
                        {
                            nmbrColor =  (rdr.ResType == ResourceType.HP)?Color.Red:Color.DarkGray;
                        }
                        else
                        {
                            nmbrColor = Color.Gray;
                        }

                        DrawText(portraitPoint.X+ DmgIconSize.Width
                            , pointY
                            , gfx
                            , Math.Floor((double)(ent.Val ?? 0)).ToString() +
                              ((ent is DirectDamage damage && damage.IsCrit) ? $" crit" : "")
                            , new SolidBrush(nmbrColor)
                            , new Font("Tahoma",BarFontSize,FontStyle.Bold));

                        j++;
                    }
                }

                //Buffs
                j = 0;
                foreach (var buff in unit.Buffs)
                {
                    var buffPoint = new Point(portraitPoint.X + PortraitSize.Width,
                        portraitPoint.Y + j * ElemSizeMini.Height);
                    gfx.DrawImage(new Bitmap(Utl.LoadBitmap(buff.CustomIconName ?? buff.Effects.First().GetType().Name), ElemSizeMini), buffPoint);
                    gfx.DrawRectangle(new Pen(buff.Type == Buff.ModType.Buff ? Color.Aquamarine : buff.Type == Buff.ModType.Debuff ? Color.Brown : Color.Violet, 1), buffPoint.X, buffPoint.Y, ElemSizeMini.Width, ElemSizeMini.Height);

                    //duration
                    DrawText(buffPoint.X
                        , buffPoint.Y + ElemSizeMini.Height - (int)Math.Round(BarFontSize * 1.3)
                        , gfx
                        , buff.DurationLeft.ToString()
                        , new SolidBrush(Color.Aqua)
                        , new Font("Tahoma", (int)Math.Round(BarFontSize * 0.8), FontStyle.Bold), true);
                    //shield val
                    double? shieldVal =
                        (buff.Effects.FirstOrDefault(x => x is EffShield)?.Value);
                    if (shieldVal > 0)
                        DrawText(buffPoint.X
                            , buffPoint.Y + (int)(ElemSizeMini.Height * 0.1)
                            , gfx
                            , (Math.Ceiling((double)(shieldVal ?? 0))).ToString()
                            , new SolidBrush(Color.GreenYellow)
                            , new Font("Tahoma", (int)Math.Round(BarFontSize * 0.8), FontStyle.Bold), true);
                    //stacks
                    if (buff.MaxStack > 1)
                    {
                        DrawText(buffPoint.X
                            , buffPoint.Y + (int)(ElemSizeMini.Height * 0.1)
                            , gfx
                            , buff.Stack.ToString() + "/" + buff.MaxStack.ToString()
                            , new SolidBrush(Color.Coral)
                            , new Font("Tahoma", (int)Math.Round(BarFontSize * 0.8), FontStyle.Bold), true);
                    }
                    j++;

                }

                //ConditionsBuffs
                j = 0;
                foreach (var buff in unit.GetConditionMods(null,null).Where(x => x is ConditionMod))
                {
                    var buffPoint = new Point(portraitPoint.X + (j * ElemSizeMini.Width),
                        portraitPoint.Y + PortraitSize.Height - ElemSizeMini.Width);
                    gfx.DrawImage(new Bitmap(Utl.LoadBitmap(buff.Mod.CustomIconName ?? buff.Mod.Effects.First().GetType().Name), ElemSizeMini), buffPoint);
                    gfx.DrawRectangle(new Pen(Color.Blue, 1), buffPoint.X, buffPoint.Y, ElemSizeMini.Width, ElemSizeMini.Height);


                    j++;

                }



                if (step.Parent.CurrentFight?.CurrentWave != null)
                {
                    Color fntColor;
                    if (step.Events.Any(x => x is ModActionValue && x.Val < 0 && x.TargetUnit == unit))
                    {
                        fntColor = Color.Red;
                    }
                    else if (step.Events.Any(x => x is ModActionValue && x.Val > 0 && x.TargetUnit == unit))
                    {
                        fntColor = Color.GreenYellow;
                    }
                    else
                    {
                        fntColor = Color.Violet;
                    }




                    //AV
                    DrawText(portraitPoint.X + 5, portraitPoint.Y + (int)(PortraitSize.Height * 0.4), gfx, Math.Ceiling(unit.GetActionValue(null)).ToString(), new SolidBrush(fntColor), new Font("Tahoma", DefaultFontSize), true);
                }

                if (step.Parent.CurrentFight != null)
                    //Special text
                    DrawText(portraitPoint.X + 5, portraitPoint.Y + (int)(PortraitSize.Height * 0.6), gfx, unit.Fighter.GetSpecialText(), new SolidBrush(Color.Chartreuse), new Font("Tahoma", (int)Math.Round((double)DefaultFontSize /2)), true);

                i++;
            }
        }
        /// <summary>
        /// text in the middle
        /// </summary>
        private static void DrawCenterText(Graphics gfx, string text, Brush brush = null)
        {
            DrawText(CombatImgSize.Width / 2 - text.Length * DefaultFontSizeSpace, CenterTextY, gfx, text, brush);
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
        private static void DrawText(int x, int y, Graphics gfx, string text, Brush brush = null, Font font = null, bool drawBack = false)
        {
            if (string.IsNullOrEmpty(text))
                return;

            brush ??= new SolidBrush(clrDefault);
            font ??= new("Tahoma", DefaultFontSize, FontStyle.Bold);

            if (drawBack)
                gfx.FillRectangle(new SolidBrush(Color.FromArgb(155, 50, 50, 50)), x + 3, y, (int)Math.Round(text.Length * (double)font.Size * 0.7), (int)Math.Round(font.Size * 1.6));
            gfx.DrawString(text, font, brush, new Point(x, y));



        }


    }
}
