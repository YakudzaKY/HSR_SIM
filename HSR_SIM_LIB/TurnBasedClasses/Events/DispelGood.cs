using System;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

internal class DispelGood : Event
{
    public DispelGood(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"{SourceUnit.Name} dispel some buff on {TargetUnit.Name}  with {AbilityValue.Name}";
    }

    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
        {
            var buffToDispell =
                TargetUnit.Buffs.FirstOrDefault(x => x.Type is Buff.BuffType.Buff && x.Dispellable);
            if (buffToDispell != null)
                ChildEvents.Add(new RemoveBuff(ParentStep, Source, SourceUnit)
                    { TargetUnit = TargetUnit, AbilityValue = AbilityValue, BuffToApply = buffToDispell });
        }

        base.ProcEvent(revert);
    }
}