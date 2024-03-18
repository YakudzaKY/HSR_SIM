using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

/// <summary>
///     Energy gain. Affected by Energy regen rate.
///     For raw energy gain use ResourceGain instead
/// </summary>
public class EnergyGain : Event
{
    public EnergyGain(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    //scale energy gain by energy regen rate
    private bool IsRawEnergy { get; } = false;

    public override string GetDescription()
    {
        return $"{TargetUnit.Name} Gain energy:{Value:f} * regenRate = {RealValue:f} source: {Source.GetType().Name}";
    }


    public override void ProcEvent(bool revert)
    {
        if (RealValue == null)
        {
            RealValue = Value * (IsRawEnergy ? 1 : TargetUnit.EnergyRegenPrc(this));
            RealValue = Math.Min((double)RealValue, TargetUnit.Stats.BaseMaxEnergy - TargetUnit.CurrentEnergy);
        }

        TargetUnit.CurrentEnergy += (double)(revert ? -RealValue : RealValue);
        base.ProcEvent(revert);
    }
}