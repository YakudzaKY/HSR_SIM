using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     Increase crit chance by x%
/// </summary>
public class EffCritPrc : Effect
{
    public override void OnApply(Event ent, Buff buff, Unit target = null)
    {
        (target ?? buff.CarrierUnit).ResetCondition(Condition.ConditionCheckParam.CritRate);
        base.OnApply(ent, buff, target);
    }

    public override void OnRemove(Event ent, Buff buff, Unit target = null)
    {
        (target ?? buff.CarrierUnit).ResetCondition(Condition.ConditionCheckParam.CritRate);
        base.OnRemove(ent, buff, target);
    }
}