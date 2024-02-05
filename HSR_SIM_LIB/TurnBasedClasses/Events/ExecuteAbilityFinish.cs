using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class ExecuteAbilityFinish : Event
{
    public ExecuteAbilityFinish(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"{SourceUnit.Name} finish execute {ParentStep.ActorAbility.Name}";
    }
}