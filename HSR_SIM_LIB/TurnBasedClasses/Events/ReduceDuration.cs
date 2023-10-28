using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    /// <summary>
    /// reduce duration of dot,buff etc
    /// </summary>
    public class ReduceDuration : ModEventTemplate
    {
        public ReduceDuration(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return null;
        }

        public override void ProcEvent(bool revert)
        {
           
            //reduce MOD turns durationleft


            Modification.DurationLeft -= !revert ? 1 : -1;
            if (!TriggersHandled && Modification.DurationLeft <= 0)
            {
                DispelMod(Modification, true);

            }

            base.ProcEvent(revert);

        }
    }
}
