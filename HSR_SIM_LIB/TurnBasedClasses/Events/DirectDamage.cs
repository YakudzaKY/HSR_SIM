using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;


namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    // direct damage dealed
    public class DirectDamage:DamageEventTemplate
    {
        public bool IsCrit { get; set; }
        public DirectDamage(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return "Dealing damage" + (IsCrit ? " (CRITICAL)" : "") +
                   $" overall={Val:f} to_barrier={RealBarrierVal:f} to_hp={RealVal:f}";
        }

        public override void ProcEvent(bool revert)
        {
            
            DamageWorks(revert);
            base.ProcEvent(revert);
        }
    }
}
