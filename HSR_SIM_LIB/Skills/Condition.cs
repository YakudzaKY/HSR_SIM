using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class Condition

{
    public enum ConditionCheckExpression
    {
        EqualOrMore,
        EqualOrLess,
        More,
        Less,
        Exists
    }

    /// <summary>
    ///     options that we can check in condition buffs
    ///     !!!! if you add some rows here do not forget add reset trigger to buff effect!!!!
    ///     For example check EffCritPrc-> OnApply and Remove triggers
    /// </summary>
    public enum ConditionCheckParam
    {
        Spd,
        Weakness,
        CritRate,
        HpPrc,
        Buff,
        AnyDebuff,
        Resource
    }

    private bool truly;
    public bool NeedRecalculate { get; set; } = true;
    
    public ConditionCheckExpression ConditionExpression { get; init; }
    public ConditionCheckParam ConditionParam { get; init; }
    public Ability.ElementEnm? ElemValue { get; init; }
    public Resource.ResourceType? ResourceValue { get; init; }

    public double? Value { get; init; }
    public AppliedBuff AppliedBuffValue { get; init; }

    /// <summary>
    ///     get list of unit affected by condition buff
    /// </summary>
    /// <returns></returns>
    /// <summary>
    ///     Expression are true?
    /// </summary>
    /// <param name="parentBuff">Buff for check(who affected to reset condition NeedCalculate)</param>
    /// <param name="targetUnit"></param>
    /// <param name="excludeCondition">prevent from recursion</param>
    /// <param name="ent"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool Truly(PassiveBuff parentBuff, Unit targetUnit = null, List<Condition> excludeCondition = null,
        Event ent = null)
    {
        if (excludeCondition == null)
            excludeCondition = new List<Condition>();
        excludeCondition.Add(this);
        //if target check, then check conditions by target. Else get parent
        var untToCheck = parentBuff?.IsTargetCheck ?? false ? ent?.TargetUnit : targetUnit ?? parentBuff?.CarrierUnit;
        if (untToCheck == null)
            return false;

        bool res;
        //if NeedRecalc condition buff or is target check (or no buff)
        if (NeedRecalculate || (parentBuff?.IsTargetCheck ?? true))
        {
            res = ConditionParam switch
            {
                ConditionCheckParam.Spd => CheckExpression(untToCheck.GetSpeed(ent:ent, excludeCondition:excludeCondition).Result),
                ConditionCheckParam.CritRate => CheckExpression(
                    untToCheck.CritChance(ent:ent, excludeCondition:excludeCondition).Result),
                ConditionCheckParam.HpPrc => untToCheck.GetMaxHp(ent:ent, excludeCondition:excludeCondition).Result != 0 &&
                                             CheckExpression(untToCheck.GetHpPrc(ent:ent, excludeCondition:excludeCondition).Result),
                ConditionCheckParam.Resource => ResourceValue!=null && CheckExpression(untToCheck.GetRes((Resource.ResourceType)ResourceValue).ResVal),
                ConditionCheckParam.Weakness => untToCheck.GetWeaknesses(ent, excludeCondition)
                                                    .Any(x => x == (ElemValue ?? ent?.SourceUnit.Fighter.Element))
                                                == (ConditionExpression ==
                                                    ConditionCheckExpression.Exists),
                ConditionCheckParam.Buff => untToCheck.AppliedBuffs.Any(x =>
                                                x.Reference == AppliedBuffValue)
                                            == (ConditionExpression ==
                                                ConditionCheckExpression.Exists),
                ConditionCheckParam.AnyDebuff => untToCheck.AppliedBuffs.Any(x =>
                                                     x.Type is Buff.BuffType.Debuff
                                                         or Buff.BuffType.Dot)
                                                 == (ConditionExpression ==
                                                     ConditionCheckExpression.Exists),
                _ => throw new NotImplementedException()
            };
            NeedRecalculate = false;


            //reset depended conditions/buffs if switched "truly"
            if (res != truly && parentBuff != null)
                foreach (var target in parentBuff.AffectedUnits())
                    if (truly)
                        foreach (var eff in parentBuff.Effects)
                        {
                            eff.BeforeApply(ent, parentBuff, target);
                            eff.OnApply(ent, parentBuff, target);
                        }
                    else
                        foreach (var eff in parentBuff.Effects)
                        {
                            eff.BeforeRemove(ent, parentBuff, target);
                            eff.OnRemove(ent, parentBuff, target);
                        }

            truly = res;
        }
        else
        {
            res = truly;
        }

        return res;
    }

    private bool CheckExpression(double targetVal)
    {
        if (ConditionExpression == ConditionCheckExpression.EqualOrMore)
            return targetVal >= Value;
        if (ConditionExpression == ConditionCheckExpression.EqualOrLess)
            return targetVal <= Value;
        if (ConditionExpression == ConditionCheckExpression.More)
            return targetVal > Value;
        if (ConditionExpression == ConditionCheckExpression.Less)
            return targetVal < Value;
        throw new NotImplementedException();
    }
}