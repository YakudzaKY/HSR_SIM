﻿using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//reset action value
public class ResetAV : Event
{
    public ResetAV(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"{TargetUnit?.Name} reset action value";
    }

    public override void ProcEvent(bool revert)
    {
        //reset av
        if (!TriggersHandled)
            Value = TargetUnit.Stats.PerformedActionValue;
        if (!revert)
            TargetUnit.Stats.ResetAV();
        else
            TargetUnit.Stats.PerformedActionValue = (double)Value;


        base.ProcEvent(revert);
    }
}