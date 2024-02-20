using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    internal class DefeatHandled(Step parentStep, ICloneable source, Unit sourceUnit)
        : Event(parentStep, source, sourceUnit)
    {
        public override string GetDescription()
        {
            return $"{TargetUnit.Name} avoid the defeat";
        }

        public override void ProcEvent(bool revert)
        {

            if (!TriggersHandled)
            {
              //add to the end of list let the other events work before this
                ParentStep.Events.Add(new SetLiveStatus(ParentStep, SourceUnit, SourceUnit)
                { ToState = Unit.LivingStatusEnm.WaitingForFollowUp, TargetUnit = TargetUnit });

            }




            base.ProcEvent(revert);
        }
    }
}
