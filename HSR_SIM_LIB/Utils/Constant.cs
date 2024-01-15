using System;
using System.Drawing;

namespace HSR_SIM_LIB.Utils;

public static class Constant
{
    public static readonly string MsgLog = "LOG";
    public static readonly string MsgDebug = "DEBUG";
    public static readonly int MaxSp = 5;
    public static readonly int MaxTp = 5;


    public static readonly Size CombatImgSize = new(1200, 600);
    public static readonly Size PortraitSize = new(CombatImgSize.Width / 10, CombatImgSize.Height / 5);
    public static readonly Size PortraitSizeMini = new(CombatImgSize.Width / 30, CombatImgSize.Height / 15);
    public static readonly Size ElemSizeMini = new(CombatImgSize.Width / 50, CombatImgSize.Height / 25);

    public static readonly Size HealthBarSize =
        new(CombatImgSize.Width / 10, (sbyte)Math.Round((double)(CombatImgSize.Height / 30)));

    public static readonly Size EnergyBarSize =
        new(CombatImgSize.Width / 10, (sbyte)Math.Round((double)(CombatImgSize.Height / 40)));

    /// <summary>
    ///     total portrait size
    /// </summary>
    public static readonly Size TotalUnitSize =
        new(PortraitSize.Width, PortraitSize.Height + HealthBarSize.Height + EnergyBarSize.Height);

    //Some of sizes or coords
    public static readonly sbyte BarFontSize = (sbyte)Math.Round((double)(CombatImgSize.Height / 65));
    public static readonly sbyte DefaultFontSize = (sbyte)Math.Round((double)(CombatImgSize.Height / 40));
    public static readonly sbyte DefaultFontSizeSpace = (sbyte)Math.Round(DefaultFontSize * 0.4);
    public static readonly short CenterTextY = (short)Math.Round(CombatImgSize.Height * 0.4);

    public static readonly short UnitSpaceSize =
        (short)Math.Round((double)((CombatImgSize.Width - 5 * TotalUnitSize.Width) / 5));

    public static readonly sbyte StartQueuefontSize = (sbyte)(DefaultFontSize * 0.90);
    public static readonly sbyte StartQueuefontSizeSpc = (sbyte)Math.Round(StartQueuefontSize / 7.5);
    public static readonly short LeftSideWithSpace = (short)(CombatImgSize.Width / 80);
    public static readonly short TopSideWithSpace = (short)(CombatImgSize.Width / 80);

    public static readonly short BottomSideWithSpace = (short)(CombatImgSize.Height - TotalUnitSize.Height -
                                                               CombatImgSize.Height / 40);

    public static readonly short TopSideForQueue = (short)(CombatImgSize.Height * 0.4);
    public static readonly short PartyResourceX = (short)(CombatImgSize.Width - CombatImgSize.Width / 8);
    public static readonly short PartyResourceY = (short)(CombatImgSize.Height / 2);
    public static readonly Color clrGreen = Color.FromArgb(87, 166, 74);
    public static readonly Color clrDefault = Color.FromArgb(220, 220, 170);
    public static readonly short fHCycleAvFirst = 150;
    public static readonly short fHCycleAvNext = 150;
    public static readonly short txtNameSize = 12;
    public static readonly Size DmgIconSize = new(14, 14);
}