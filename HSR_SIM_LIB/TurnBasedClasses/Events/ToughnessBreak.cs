using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//Break the shield
public class ToughnessBreak(Step parent, ICloneable source, Unit sourceUnit)
    : DamageEventTemplate(parent, source, sourceUnit)
{
    public override string GetDescription()
    {
        return TargetUnit.Name + " shield broken " +
               $" overall={Val:f} to_hp={RealVal:f}";
    }

    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
        {
            ModActionValue delayAv = new(ParentStep, Source, SourceUnit)
            {
                TargetUnit = TargetUnit,
                Val = -TargetUnit.GetActionValue(this) * 0.25
            }; //default delay
            ChildEvents.Add(delayAv);
            TryDebuff(SourceUnit.Fighter.WeaknessBreakDebuff, 1.5);
            // https://honkai-star-rail.fandom.com/wiki/Toughness


            DamageWorks(revert);
            base.ProcEvent(revert);
        }
    }
}