using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

/// <summary>
///     reduce duration of dot,buff etc
/// </summary>
public class ReduceDuration(Step parent, ICloneable source, Unit sourceUnit)
    : BuffEventTemplate(parent, source, sourceUnit)
{
    public override string GetDescription()
    {
        return null;
    }

    public override void ProcEvent(bool revert)
    {
        //reduce MOD turns durationleft


        BuffToApply.DurationLeft -= !revert ? 1 : -1;
        if (!TriggersHandled && BuffToApply.DurationLeft <= 0) DispelMod(BuffToApply, true);

        base.ProcEvent(revert);
    }
}