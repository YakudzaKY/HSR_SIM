using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class AttemptEffect : BuffEventTemplate
{
    /// <summary>
    ///     attempt place debuff on target
    /// </summary>
    /// <param name="parentStep"></param>
    /// <param name="source"></param>
    /// <param name="sourceUnit"></param>
    public AttemptEffect(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
    {
    }

    public double BaseChance { get; set; }

    public override string GetDescription()
    {
        return $"{SourceUnit.Name} try apply effect on {TargetUnit.Name}";
    }

    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
            TryDebuff(AppliedBuffToApply, BaseChance);

        base.ProcEvent(revert);
    }
}