using System;
using System.Linq;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class ToughnessShred : Event
{
    public ToughnessShred(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
            if ((TargetUnit.Buffs.All(x => x.Effects.All(y => y is not EffBarrier)) &&
                 TargetUnit.GetWeaknesses(this).Any(x => x == AbilityValue.Element)) || AbilityValue.IgnoreWeakness)
                ChildEvents.Add(new ResourceDrain(null, null, AbilityValue.Parent.Parent)
                {
                    ParentStep = ParentStep,
                    TargetUnit = TargetUnit,
                    ResType = Resource.ResourceType.Toughness,
                    Val = Val,
                    AbilityValue = AbilityValue
                });
        base.ProcEvent(revert);
    }

    public override string GetDescription()
    {
        return $"shred {TargetUnit.Name:s} toughness by {Val:f}";
    }
}