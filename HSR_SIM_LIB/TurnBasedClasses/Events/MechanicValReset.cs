using System;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//Reset mechanics
public class MechanicValReset(Step parent, ICloneable source, Unit sourceUnit) : Event(parent, source, sourceUnit)
{
    public Ability AbilityValue { get; init; }

    public override string GetDescription()
    {
        return $"{SourceUnit?.Name} {AbilityValue.Name} mechanic reset";
    }

    public override void ProcEvent(bool revert)
    {
        if (!revert)
        {
            if (!TriggersHandled)
                Value = SourceUnit.Fighter.Mechanics.Values[AbilityValue];
            SourceUnit.Fighter.Mechanics.Values[AbilityValue] = 0;
        }
        else
        {
            SourceUnit.Fighter.Mechanics.Values[AbilityValue] = Value ?? 0;
        }

        
        base.ProcEvent(revert);
    }
}