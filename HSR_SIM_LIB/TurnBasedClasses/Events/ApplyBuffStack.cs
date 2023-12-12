using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    internal class ApplyBuffStack:BuffEventTemplate
    {
        public int Stacks;
        public int RealStacks;
        public ApplyBuffStack(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return $"Apply buff stack on {TargetUnit.Name} stack+ ={Stacks}. Source: {Source?.GetType()?.Name:s}";
        }

        
        public override void ProcEvent(bool revert)
        {

            if (!TargetUnit.IsAlive) return;
            if (!TriggersHandled)
                RealStacks = Math.Min(BuffToApply.MaxStack - TargetUnit.GetStacks(BuffToApply), Stacks);

            TargetUnit.AddStack( BuffToApply,!revert?1:-1*RealStacks);
            
        
            

            base.ProcEvent(revert);
        }
    }
}
