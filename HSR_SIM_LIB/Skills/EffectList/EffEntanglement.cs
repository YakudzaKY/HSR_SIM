using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Skills.EffectList
{
    public class EffEntanglement : Effect
    {
        public override void OnNaturalExpire(Event ent, Buff mod)
        {

            if (mod == mod.Caster.Fighter.ShieldBreakMod)
            {
                var dotProcEvent = new ToughnessBreakDoTDamage(ent.ParentStep, mod.Caster, mod.Caster)
                {
                    CalculateValue = this.CalculateValue,
                    TargetUnit = ent.TargetUnit,
                    Modification = mod,
                    AbilityValue = mod.AbilityValue
                };
                ent.ChildEvents.Add(dotProcEvent);
            }
            else
            {
                var dotProcEvent = new DoTDamage(ent.ParentStep, mod.Caster, mod.Caster)
                {
                    CalculateValue = this.CalculateValue,
                    TargetUnit = ent.TargetUnit,
                    Modification = mod,
                    AbilityValue = mod.AbilityValue
                };
                ent.ChildEvents.Add(dotProcEvent);
            }

            base.OnNaturalExpire(ent,mod);
        }
    }
}
