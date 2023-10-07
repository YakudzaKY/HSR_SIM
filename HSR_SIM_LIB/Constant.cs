using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    public static class Constant
    {      
        public static readonly string MsgLog = "LOG";
        public static readonly int MaxSp = 5;
        public static readonly int MaxTp = 5;


        public static readonly Size CombatImgSize = new Size(800, 400);
        public static readonly Size PortraitSize = new Size((CombatImgSize.Width/8), (CombatImgSize.Height / 4));
        public static readonly Size HealthBarSize = new Size((CombatImgSize.Width / 8), (sbyte)(Math.Round((double)(CombatImgSize.Height / 26.66))) );
        public static readonly Size EnergyBarSize = new Size((CombatImgSize.Width / 8), (sbyte)(Math.Round((double)(CombatImgSize.Height / 40))));
        /// <summary>
        /// total portrait size
        /// </summary>
        public static readonly Size TotalUnitSize = new Size(PortraitSize.Width, PortraitSize.Height+ HealthBarSize.Height+ EnergyBarSize.Height);

        //Some of sizes or coords
        public static readonly sbyte BarFontSize = (sbyte) (Math.Round((double)(CombatImgSize.Height / 57.14)));
        public static readonly sbyte DefaultFontSize = (sbyte)(Math.Round((double)(CombatImgSize.Height / 28.5)));
        public static readonly sbyte DefaultFontSizeSpace = (sbyte)(Math.Round((double)(DefaultFontSize * 1.35)));
        public static readonly short CenterTextY = (short)Math.Round((double)(CombatImgSize.Height * 0.4));
        public static readonly sbyte UnitSpaceSize = (sbyte)Math.Round((double)((CombatImgSize.Width - (5 * TotalUnitSize.Width)) / 5));
        public static readonly sbyte StartQueuefontSize = (sbyte)Math.Round((double)(CombatImgSize.Height / 30));
        public static readonly sbyte StartQueuefontSizeSpc = (sbyte)Math.Round((double)(StartQueuefontSize / 7.5));
        public static readonly short LeftSideWithSpace = (short)(CombatImgSize.Width / 80);
        public static readonly short TopSideWithSpace = (short)(CombatImgSize.Width / 80);
        public static readonly short BottomSideWithSpace =(short)( CombatImgSize.Height - TotalUnitSize.Height - (int)(CombatImgSize.Height / 40));
        public static readonly short TopSideForQueue = (short)(CombatImgSize.Height * 0.2);
        public static readonly short PartyResourceX = (short) ( CombatImgSize.Width - (int) (CombatImgSize.Width/8));
        public static readonly short PartyResourceY = (short)(CombatImgSize.Height / 2);

    }
}
