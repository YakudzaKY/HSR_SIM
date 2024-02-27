using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

internal class ApplyBuffStack : BuffEventTemplate
{
    public int RealStacks;
    public int Stacks;

    public ApplyBuffStack(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"Apply buff stack on {TargetUnit.Name} stack+ ={Stacks}. Source: {Source?.GetType()?.Name:s}";
    }


    public override void ProcEvent(bool revert)
    {
        if (TargetUnit.IsAlive)
        {
            if (!TriggersHandled)
                RealStacks = Math.Min(AppliedBuffToApply.MaxStack - TargetUnit.GetStacks(AppliedBuffToApply), Stacks);
            TargetUnit.AddStack(AppliedBuffToApply, !revert ? 1 : -1 * RealStacks);
        }


        base.ProcEvent(revert);
    }
}