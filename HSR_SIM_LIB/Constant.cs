using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    public class Constant
    {
        public static readonly Constant Instance = new Constant();
        public static readonly string MsgLog = "LOG";
        public static readonly int MaxSp = 5;
        public static readonly int MaxTp = 5;
        public enum unitHostility
        {
            Friendly,
            Hostile
        }
        public static readonly Size PortraitSize = new Size(100, 100);        
        public static readonly Size HealthBarSize = new Size(100, 15);
        public static readonly Size CombatImgSize = new Size(800, 400);        
        /// <summary>
        /// total
        /// </summary>
        public static readonly Size TotalUnitSize = new Size(PortraitSize.Width, PortraitSize.Height+ HealthBarSize.Height);
    }
}
