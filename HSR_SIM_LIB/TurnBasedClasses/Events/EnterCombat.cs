using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    // command to start battle(when combat technique used)
    public class EnterCombat:Event
    {
        public EnterCombat(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return "entering the combat...";
        }

        public override void ProcEvent(bool revert)
        {
            
            //entering combat
            ParentStep.Parent.DoEnterCombat = !revert;
            base.ProcEvent(revert);
        }
    }
}
