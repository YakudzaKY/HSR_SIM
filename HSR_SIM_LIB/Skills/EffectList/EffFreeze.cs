﻿using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Skills.EffectList;

public class EffFreeze : Effect
{
    public override void OnNaturalExpire(Event ent, Buff mod)
    {
        if (mod == mod.Caster.Fighter.ShieldBreakMod)
        {
            var dotProcEvent = new ToughnessBreakDoTDamage(ent.Parent, mod.Caster, mod.Caster)
            {
                CalculateValue = CalculateValue,
                TargetUnit = ent.TargetUnit,
                Modification = mod,
                AbilityValue = mod.AbilityValue
            };
            ent.ChildEvents.Add(dotProcEvent);
        }
        else
        {
            var dotProcEvent = new DoTDamage(ent.Parent, mod.Caster, mod.Caster)
            {
                CalculateValue = CalculateValue,
                TargetUnit = ent.TargetUnit,
                Modification = mod,
                AbilityValue = mod.AbilityValue
            };
            ent.ChildEvents.Add(dotProcEvent);
        }

        base.OnNaturalExpire(ent, mod);
    }
}