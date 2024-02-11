using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// unit enter on the battlefield
public class UnitEnteringBattle : Event
{
    public UnitEnteringBattle(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"{TargetUnit?.Name:s} joined the battle";
    }

    public override void ProcEvent(bool revert)
    {
        TargetUnit.OnEnteringBattle();
        TargetUnit.ParentTeam.ResetRoles();
        //need reset enemy team roles coz some roles eff depend on enemy count
        TargetUnit.EnemyTeam.ResetRoles();
        base.ProcEvent(revert);
    }
}