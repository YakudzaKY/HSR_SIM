﻿using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     Entanglement debuff
/// </summary>
public class EffEntanglement : EffDotTemplate
{
    public override Ability.ElementEnm Element { get; init; } = Ability.ElementEnm.Quantum;

    public override void OnNaturalExpire(Event ent, Buff buff)
    {
        if (buff.Reference == buff.SourceUnit.Fighter.WeaknessBreakDebuff)
        {
            var dotProcEvent = new ToughnessBreakDoTDamage(ent.ParentStep, buff.SourceUnit, buff.SourceUnit, Element)
            {
                CalculateValue = DoTCalculateValue,
                TargetUnit = ent.TargetUnit,
                BuffThatDamage = buff
            };
            ent.ChildEvents.Add(dotProcEvent);
        }
        else
        {
            var dotProcEvent = new DoTDamage(ent.ParentStep, buff.SourceUnit, buff.SourceUnit, Element)
            {
                CalculateValue = DoTCalculateValue,
                TargetUnit = ent.TargetUnit,
                BuffThatDamage = buff
            };
            ent.ChildEvents.Add(dotProcEvent);
        }

        base.OnNaturalExpire(ent, buff);
    }
}