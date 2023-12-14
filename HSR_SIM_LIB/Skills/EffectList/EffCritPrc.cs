using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Skills.EffectList;

public class EffCritPrc : Effect
{
    public override void OnApply(Event ent, Buff buff)
    {
        buff.Owner.ResetCondition(ConditionBuff.ConditionCheckParam.CritRate);
        base.OnApply(ent, buff);
    }

    public override void OnRemove(Event ent, Buff buff)
    {
        buff.Owner.ResetCondition(ConditionBuff.ConditionCheckParam.CritRate);
        base.OnRemove(ent, buff);
    }
}