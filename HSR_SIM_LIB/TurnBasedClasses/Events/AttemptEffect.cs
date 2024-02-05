using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    public class AttemptEffect:BuffEventTemplate
    {
        /// <summary>
        /// attempt place debuff on target
        /// </summary>
        /// <param name="parentStep"></param>
        /// <param name="source"></param>
        /// <param name="sourceUnit"></param>
        public AttemptEffect(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return $"{SourceUnit.Name} try apply effect on {TargetUnit.Name}";
        }

        public double BaseChance { get; set; }
        public override void ProcEvent(bool revert)
        {
            if (!TriggersHandled)
                TryDebuff(BuffToApply, BaseChance);
            
            base.ProcEvent(revert);
        }
    }
}
