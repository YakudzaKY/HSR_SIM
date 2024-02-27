using System;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public abstract class BuffEventTemplate(Step parent, ICloneable source, Unit sourceUnit)
    : Event(parent, source, sourceUnit)
{
    public AppliedBuff AppliedBuffToApply { get; init; }
}