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

    public override string GetDescription()
    {
        return $"{TargetUnit.Name} Gain energy:{Val:f} * regenRate = {RealVal:f}";
    }


    public override void ProcEvent(bool revert)
    {
        if (RealVal == null)
        {
            RealVal = Val * TargetUnit.EnergyRegenPrc(this);
            RealVal = Math.Min((double)RealVal, TargetUnit.Stats.BaseMaxEnergy - TargetUnit.CurrentEnergy);
        }

        TargetUnit.CurrentEnergy += (double)(revert ? -RealVal : RealVal);
        base.ProcEvent(revert);
    }
}