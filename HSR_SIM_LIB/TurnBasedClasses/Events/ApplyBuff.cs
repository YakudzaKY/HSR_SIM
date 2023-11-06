using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    //apply buff debuff dot etc
    public class ApplyBuff : ModEventTemplate
    {
     
        public ApplyBuff(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return $"Apply buff on {TargetUnit.Name}. Source: {Source?.GetType()?.Name:s}";
        }

        //Base Chance to apply debuff
        public double BaseChance { get; set; }

        public override void ProcEvent(bool revert)
        {

            //Apply mod
            BuffToApply.AbilityValue ??= AbilityValue;
            if (!TargetUnit.IsAlive) return;
            //calc value first
            foreach (var modEffect in BuffToApply.Effects.Where(modEffect => modEffect.CalculateValue != null && modEffect.Value == null))
            {
                modEffect.Value = modEffect.CalculateValue(this);
            }

            if (!revert)
                TargetUnit.ApplyBuff(this,BuffToApply);
            else
                TargetUnit.RemoveBuff(this,BuffToApply);

            base.ProcEvent(revert);
        }
        
    }
}
