using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_LIB.Skills;

public class PassiveBuff : Buff
{

    public PassiveBuff(Unit parent,object sourceObject) : base(parent)
    {
        CarrierUnit = parent;
        SourceObject = sourceObject;
    }

    public object Target { get; set; } //in most cases target==parent, but when target is full team then not



    public Condition WorkCondition { get; init; }
    public bool IsTargetCheck { get; init; }
    public  object SourceObject { get; }

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