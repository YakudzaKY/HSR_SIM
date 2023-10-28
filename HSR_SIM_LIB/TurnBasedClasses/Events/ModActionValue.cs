using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    //modify Action Value
    public class ModActionValue : Event
    {
        public ModActionValue(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return $"Reduce {TargetUnit?.Name:s} action value on {Val:f}";
        }

        public override void ProcEvent(bool revert)
        {
            
            //SetAV

            //if no target - reduce all units
            if (TargetUnit != null)
            {
                TargetUnit.Stats.PerformedActionValue += (double)(revert ? -Val : Val);
            }
            else
            {

                foreach (Unit unit in ParentStep.Parent.CurrentFight.AllAliveUnits)
                {
                    unit.Stats.PerformedActionValue += (double)(revert ? -Val : Val);
                }
            }
            base.ProcEvent(revert);
        }
    }
}
