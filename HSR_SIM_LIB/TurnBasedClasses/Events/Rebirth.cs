using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

internal class Rebirth : Event
{
    public Rebirth(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"{TargetUnit.Name} got resurrected";
    }
}