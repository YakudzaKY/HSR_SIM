﻿using HSR_SIM_LIB.Skills.ReadyBuffs;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Skills.EffectList;

public class EffEntanglement : Effect
{
    public override void OnNaturalExpire(Event ent, Buff mod)
    {
        if (mod == mod.Caster.Fighter.ShieldBreakMod||mod is BuffEntanglementWB)
        {
            var dotProcEvent = new ToughnessBreakDoTDamage(ent.ParentStep, mod.Caster, mod.Caster)
            {
                CalculateValue = CalculateValue,
                TargetUnit = ent.TargetUnit,
                Modification = mod
            };
            ent.ChildEvents.Add(dotProcEvent);
        }
        else
        {
            var dotProcEvent = new DoTDamage(ent.ParentStep, mod.Caster, mod.Caster)
            {
                CalculateValue = CalculateValue,
                TargetUnit = ent.TargetUnit,
                Modification = mod
            };
            ent.ChildEvents.Add(dotProcEvent);
        }

        base.OnNaturalExpire(ent, mod);
    }
}