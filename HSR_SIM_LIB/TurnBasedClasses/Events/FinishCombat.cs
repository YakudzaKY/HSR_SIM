using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    //Combat was finished
    internal class FinishCombat:Event
    {
        public FinishCombat(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            throw new NotImplementedException();
        }

        public override void ProcEvent(bool revert)
        {
           
            
            //todo: reset all passives and counters for characters
            //also reset for gear and light cones
            //
            throw new NotImplementedException();
            base.ProcEvent(revert);
        }
    }
}
