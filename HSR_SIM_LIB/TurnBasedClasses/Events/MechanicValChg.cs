using System;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//Add value to character mechanic counter
public class MechanicValChg : Event
{
    public MechanicValChg(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public Ability AbilityValue { get; set; }

    public override string GetDescription()
    {
        return $"{SourceUnit?.Name:s} mechanic counter change on  {Val:f}";
    }

    public override void ProcEvent(bool revert)
    {
        SourceUnit.Fighter.Mechanics.Values[AbilityValue] += (double)(revert ? -Val : Val);
        base.ProcEvent(revert);
    }
}