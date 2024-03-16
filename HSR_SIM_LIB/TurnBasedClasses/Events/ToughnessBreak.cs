using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//Break the shield
public class ToughnessBreak(Step parent, ICloneable source, Unit sourceUnit)
    : DamageEventTemplate(parent, source, sourceUnit)
{
    public override string GetDescription()
    {
        return TargetUnit.Name + " weakness broken " +
               $" overall={Value:f} to_hp={RealValue:f}";
    }

    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
        {
            ModActionValue delayAv = new(ParentStep, Source, SourceUnit)
            {
                TargetUnit = TargetUnit,
                Value = -TargetUnit.GetActionValue(this) * 0.25
            }; //default delay
            ChildEvents.Add(delayAv);
            // https://honkai-star-rail.fandom.com/wiki/Toughness
            ChildEvents.Add(new AttemptEffect(ParentStep, this, SourceUnit)
            {
                AppliedBuffToApply = SourceUnit.Fighter.WeaknessBreakDebuff,
                BaseChance = 1.5,
                TargetUnit = TargetUnit
            });
        }

        DamageWorks(revert);
        base.ProcEvent(revert);
    }
}