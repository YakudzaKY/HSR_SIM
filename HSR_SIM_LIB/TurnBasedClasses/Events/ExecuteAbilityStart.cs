using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class ExecuteAbilityStart : Event
{
    public ExecuteAbilityStart(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"{SourceUnit.Name} start execute {ParentStep.ActorAbility.Name}";
    }
}