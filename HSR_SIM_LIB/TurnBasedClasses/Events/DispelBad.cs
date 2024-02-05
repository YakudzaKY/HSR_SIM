using System;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class DispelBad : Event
{
    public DispelBad(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"{SourceUnit.Name} dispel some shit on {TargetUnit.Name}  with {ParentStep.ActorAbility.Name}";
    }

    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
        {
            var buffToDispell =
                TargetUnit.Buffs.FirstOrDefault(x =>
                    x.Type is Buff.BuffType.Debuff or Buff.BuffType.Dot && x.Dispellable);
            if (buffToDispell != null)
                ChildEvents.Add(new RemoveBuff(ParentStep, Source, SourceUnit)
                    { TargetUnit = TargetUnit, BuffToApply = buffToDispell });
        }

        base.ProcEvent(revert);
    }
}