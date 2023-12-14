using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Skills.EffectList;

public class EffSpeedPrc : Effect
{
    public override void OnApply(Event ent, Buff buff)
    {
        buff.Owner.ResetCondition(ConditionBuff.ConditionCheckParam.SPD);
        base.OnApply(ent, buff);
    }

    public override void OnRemove(Event ent, Buff buff)
    {
        buff.Owner.ResetCondition(ConditionBuff.ConditionCheckParam.SPD);
        base.OnRemove(ent, buff);
    }
}