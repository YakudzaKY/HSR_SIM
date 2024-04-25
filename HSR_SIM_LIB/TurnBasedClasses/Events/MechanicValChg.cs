using System;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//Add value to character mechanic counter
public class MechanicValChg(Step parent, ICloneable source, Unit sourceUnit) : Event(parent, source, sourceUnit)
{
    public Ability AbilityValue { get; init; }

    public override string GetDescription()
    {
        return $"{SourceUnit?.Name} {AbilityValue.Name} mechanic counter change on  {Value:f}";
    }

    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
            ParentStep.Parent.CalcBuffer?.Reset(SourceUnit, Condition.ConditionCheckParam.Mechanics);
        SourceUnit.Fighter.Mechanics.Values[AbilityValue] += revert ? -Value ?? 0 : Value ?? 0;
        base.ProcEvent(revert);
    }
}