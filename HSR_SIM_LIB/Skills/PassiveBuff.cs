using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_LIB.Skills;

public class PassiveBuff : Buff
{
    public PassiveBuff(Unit parent, object sourceObject) : base(parent,null,sourceObject)
    {
        CarrierUnit = parent;
    }

    public object Target { get; set; } //in most cases target==parent, but when target is full team then not

    /// <summary>
    ///     Conditions are OK
    /// </summary>
    /// <param name="parentBuff"></param>
    /// <param name="targetUnit"></param>
    /// <param name="excludeCondition"></param>
    /// <param name="ent"></param>
    /// <returns></returns>
    public bool Truly(PassiveBuff parentBuff, Unit targetUnit , List<Condition> excludeCondition ,
        Event ent )
    {
        if (ApplyConditions is null || ApplyConditions.Count == 0) return true;
        return ApplyConditions.All(x => x.Truly(parentBuff, targetUnit, excludeCondition, ent));
    }
    public List<Condition> ApplyConditions { get; init; }
    public bool IsTargetCheck { get; init; }


    /// <summary>
    ///     get list of affected targets
    /// </summary>
    /// <returns></returns>
    public Unit[] AffectedUnits()
    {
        Unit[] affectedTargets;
        if (Target is Unit utr)
            affectedTargets = new[] { utr };
        else if (Target is Team tm)
            affectedTargets = tm.Units.Where(x => x.IsAlive).ToArray();
        else if (Target is TargetTypeEnm tte)
            affectedTargets = CarrierUnit.GetTargetsForUnit(tte).ToArray();
        else
            affectedTargets = new Unit[] { };
        return affectedTargets;
    }

    /// <summary>
    ///     unit is affected by this
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool UnitIsAffected(Unit unit)
    {
        if ((Target is Unit && Target == unit)
            || (Target is Team && Target == unit.ParentTeam)
            || (Target is TargetTypeEnm &&
                CarrierUnit.GetTargetsForUnit((TargetTypeEnm)Target).Contains(unit)))
            return true;
        return false;
    }
}