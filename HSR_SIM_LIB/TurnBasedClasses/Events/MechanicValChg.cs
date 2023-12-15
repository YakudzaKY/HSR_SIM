using System;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//Add value to character mechanic counter
public class MechanicValChg : Event
{
    public Ability AbilityValue { get; set; }
    public MechanicValChg(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"{TargetUnit?.Name:s} mechanic counter change on  {Val:f}";
    }

    public override void ProcEvent(bool revert)
    {
        ((DefaultFighter)TargetUnit.Fighter).Mechanics.Values[AbilityValue] += (double)(revert ? -Val : Val);
        base.ProcEvent(revert);
    }
}