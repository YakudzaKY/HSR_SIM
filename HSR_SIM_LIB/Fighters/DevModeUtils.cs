using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils.Utils;

namespace HSR_SIM_LIB.Fighters
{
    internal static class DevModeUtils
    {
        public static bool IsCrit(Event ent)
        {
            Worker wrk = ent.ParentStep.Parent.Parent;
            if (!wrk.DevMode)
                return new MersenneTwister().NextDouble() <= ent.SourceUnit.GetCritRate(ent);
            else
            {
                string[] critStrings = { "CRIT", "not crit" };
                int devLogVal = wrk.DevModeLog.ReadNext(critStrings,$"{ent.ParentStep.GetDescription()} is crit hit on #{ent.TargetUnit.ParentTeam.Units.IndexOf(ent.TargetUnit)+1} {ent.TargetUnit.Name} ?");
                return devLogVal == 0 ? true : false;
            }


        }

    }
}
