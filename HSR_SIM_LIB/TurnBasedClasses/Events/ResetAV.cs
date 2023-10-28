using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    //reset action value
    public class ResetAV:Event
    {
        public ResetAV(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return $"{TargetUnit?.Name:s} reset action value";
        }

        public override void ProcEvent(bool revert)
        {
           
            //reset av
            TargetUnit.Stats.ResetAV();
            base.ProcEvent(revert);

        }
    }
}
