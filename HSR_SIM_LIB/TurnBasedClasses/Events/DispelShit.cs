using System;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

internal class DispelShit : Event
{
    public DispelShit(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"{SourceUnit.Name} dispel some shit on {TargetUnit.Name}  with {AbilityValue.Name}";
    }

    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
        {
            var buffToDispell =
                TargetUnit.Buffs.FirstOrDefault(x =>
                    x.Type is Buff.ModType.Debuff or Buff.ModType.Dot && x.Dispellable);
            if (buffToDispell != null)
                ChildEvents.Add(new RemoveBuff(Parent, Source, SourceUnit)
                    { TargetUnit = TargetUnit, AbilityValue = AbilityValue, BuffToApply = buffToDispell });
        }

        base.ProcEvent(revert);
    }
}