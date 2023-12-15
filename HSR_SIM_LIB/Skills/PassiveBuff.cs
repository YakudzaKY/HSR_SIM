using System.Linq;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class PassiveBuff(Unit parentUnit)
{


    public Buff AppliedBuff { get; set; }
    public object Target { get; set; } //in most cases target==parent, but when target is full team then not
    public Unit Parent { get; init; } = parentUnit;
    public bool IsTargetCheck { get; set; }

    /// <summary>
    /// get list of affected targets
    /// </summary>
    /// <returns></returns>
    public Unit[] AffectedUnits()
    {
        Unit[] affectedTargets;
        if (Target is Unit utr)
            affectedTargets = new[] { utr };
        else if (Target is Team tm)
            affectedTargets = tm.Units.Where(x => x.IsAlive).ToArray();
        else if (Target is Ability.TargetTypeEnm tte)
            affectedTargets = Parent.GetTargetsForUnit(tte).ToArray();
        else
            affectedTargets = new Unit[] { };
        return affectedTargets;
    }

    /// <summary>
    /// unit is affected by this
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool UnitIsAffected(Unit unit)
    {
        if ((Target is Unit && Target == unit)
            || (Target is Team && Target == unit.ParentTeam)
            || (Target is Ability.TargetTypeEnm &&
                Parent.GetTargetsForUnit((Ability.TargetTypeEnm)Target).Contains(unit)))
            return true;
        else
            return false;
    }
}