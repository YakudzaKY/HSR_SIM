using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    /// <summary>
    /// check target condition then proceed with defeat event 
    /// </summary>
    internal class CheckForDefeat:Event
    {
        public KeyValuePair<Unit, Unit> TargetSourcePair { get; init; }
        public CheckForDefeat(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
        {
           
        }

        public override void ProcEvent(bool revert)
        {
            if (!TriggersHandled)
            {
                //if alive but 0 hp then proc defeat
                if (TargetSourcePair.Key.IsAlive && TargetSourcePair.Key.GetRes(Resource.ResourceType.HP).ResVal == 0)
                {
                    ChildEvents.Add(new Defeat(this.ParentStep,this,TargetSourcePair.Value){TargetUnit = TargetSourcePair.Key});
                }
            }
            base.ProcEvent(revert);
        }

        public override string GetDescription()
        {
            return null;
        }
    }
}
