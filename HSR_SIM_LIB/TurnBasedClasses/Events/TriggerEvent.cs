using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class TriggerEvent : Event
{
    public TriggerEvent(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return null;
    }
}