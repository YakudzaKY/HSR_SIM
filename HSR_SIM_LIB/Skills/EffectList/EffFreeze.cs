using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     freeze debuff.
/// </summary>
public class EffFreeze : EffDotTemplate
{
    public override Ability.ElementEnm Element { get; init; } = Ability.ElementEnm.Ice;

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