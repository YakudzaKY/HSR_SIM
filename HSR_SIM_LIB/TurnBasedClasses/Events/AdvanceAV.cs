using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class AdvanceAV : Event
{
    public AdvanceAV(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"{TargetUnit?.Name:s} advance forward";
    }

    public override void ProcEvent(bool revert)
    {
        //reset av
        if (!TriggersHandled)
            Val = TargetUnit.Stats.PerformedActionValue;
        if (!revert)
            TargetUnit.Stats.PerformedActionValue = TargetUnit.GetActionValue(this);
        else
            TargetUnit.Stats.PerformedActionValue = (double)Val;


        base.ProcEvent(revert);
    }
}