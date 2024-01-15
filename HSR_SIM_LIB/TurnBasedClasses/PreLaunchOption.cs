using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB.TurnBasedClasses
{
    /*
     *some pre launch options
     * like set team points to value
     * or fill energy of team
     */
    public class PreLaunchOption
    {
        public PreLaunchOptionEnm OptionType { get; set; }
        public double Value { get; set; }
        public enum PreLaunchOptionEnm
        {
            SetEnergy,
            SetTp,
            SetSp
        }
    }
}
