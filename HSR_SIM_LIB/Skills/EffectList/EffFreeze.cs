﻿using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

public class EffFreeze : EffDotTemplate
{
    public override void OnNaturalExpire(Event ent, Buff buff)
    {
        if (buff.Reference == buff.Caster.Fighter.ShieldBreakDebuff)
        {
            var dotProcEvent = new ToughnessBreakDoTDamage(ent.ParentStep, buff.Caster, buff.Caster,Element)
            {
                CalculateValue = DoTCalculateValue,
                TargetUnit = ent.TargetUnit,
                Modification = buff
            };
            ent.ChildEvents.Add(dotProcEvent);
        }
        else
        {
            var dotProcEvent = new DoTDamage(ent.ParentStep, buff.Caster, buff.Caster, Element)
            {
                CalculateValue = DoTCalculateValue,
                TargetUnit = ent.TargetUnit,
                Modification = buff
            };
            ent.ChildEvents.Add(dotProcEvent);
        }

        base.OnNaturalExpire(ent, buff);
    }

    public override Unit.ElementEnm Element { get; init; } = Unit.ElementEnm.Ice;
}