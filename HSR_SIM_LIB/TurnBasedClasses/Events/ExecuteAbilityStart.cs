using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class ExecuteAbilityStart(Step parent, ICloneable source, Unit sourceUnit) : Event(parent, source, sourceUnit)
{
    public override string GetDescription()
    {
        return $"{SourceUnit.Name} start execute {ParentStep.ActorAbility.Name}";
    }
}