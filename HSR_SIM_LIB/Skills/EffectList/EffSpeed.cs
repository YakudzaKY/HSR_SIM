using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     increase or reduce speed by x
/// </summary>
public class EffSpeed : Effect
{
    public override void OnApply(Event ent, Buff buff, Unit target = null)
    {
        (target ?? buff.CarrierUnit).ResetCondition(Condition.ConditionCheckParam.Spd);
        base.OnApply(ent, buff, target);
    }

    public override void OnRemove(Event ent, Buff buff, Unit target = null)
    {
        (target ?? buff.CarrierUnit).ResetCondition(Condition.ConditionCheckParam.Spd);
        base.OnRemove(ent, buff, target);
    }
}