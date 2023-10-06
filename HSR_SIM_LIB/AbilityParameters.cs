using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    

    public  class AbilityParameters
    {


        public static string Name { get; internal set; }
        public static bool EnterCombat { get; internal set; }
        public static  bool IgnoreWeaknes { get; internal set; }

        public record struct Point
        {
            public double X { get; init; }
            public double Y { get; init; }
            public double Z { get; init; }
        }
    }
}
