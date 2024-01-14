using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    internal class TriggerEvent: Event
    {
        public TriggerEvent(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return null;
        }
    }
}
