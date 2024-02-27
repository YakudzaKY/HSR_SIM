using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     Max HP fixed value buff
/// </summary>
public class EffMaxHp : Effect
{
    private double GetCorrectedHp(Event ent, Buff appliedBuff)
    {
        var buffOwner = appliedBuff.CarrierUnit ?? ent.TargetUnit;
        var increasedHp = buffOwner.Stats.BaseMaxHp + (Value ?? 0 * (StackAffectValue ? appliedBuff.Stack : 1));
        return increasedHp * buffOwner.GetHpPrc(ent);
    }

    public override void BeforeApply(Event ent, Buff buff, Unit target = null)
    {
        var buffOwner = (target ?? buff.CarrierUnit) ?? ent.TargetUnit;
        buffOwner.GetRes(Resource.ResourceType.HP).ResVal += GetCorrectedHp(ent, buff);

        base.BeforeApply(ent, buff, target);
    }


    public override void BeforeRemove(Event ent, Buff buff, Unit target = null)
    {
        var buffOwner = (target ?? buff.CarrierUnit) ?? ent.TargetUnit;
        buffOwner.GetRes(Resource.ResourceType.HP).ResVal -= GetCorrectedHp(ent, buff);
        base.BeforeRemove(ent, buff,target);
    }

    public override void OnApply(Event ent, Buff buff, Unit target = null)
    {
        (target ?? buff.CarrierUnit).ResetCondition(PassiveBuff.ConditionCheckParam.HpPrc);
        base.OnApply(ent, buff, target);
    }

    public override void OnRemove(Event ent, Buff buff, Unit target = null)
    {
        (target ?? buff.CarrierUnit).ResetCondition(PassiveBuff.ConditionCheckParam.HpPrc);
        base.OnRemove(ent, buff, target);
    }
}