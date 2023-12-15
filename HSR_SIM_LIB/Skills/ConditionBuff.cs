using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_LIB.Skills;

public class ConditionBuff(Unit parentUnit) : PassiveBuff(parentUnit)
{
    public enum ConditionCheckExpression
    {
        EqualOrMore,
        EqualOrLess,
        Exists
    }


    public enum ConditionCheckParam
    {
        SPD,
        Weakness,
        CritRate,
        HPPrc,
        Buff
    }



    public ConditionRec Condition { get; set; }


    private bool CheckExpression(double targetVal)
    {
        if (Condition.CondtionExpression == ConditionCheckExpression.EqualOrMore)
            return targetVal >= Condition.Value;
        if (Condition.CondtionExpression == ConditionCheckExpression.EqualOrLess)
            return targetVal <= Condition.Value;
        throw new NotImplementedException();
    }

    private bool truly;
    public bool NeedRecalc { get; set; } = true;

    /// <summary>
    /// get list of unit affected by condition buff
    /// </summary>
    /// <returns></returns>

    /// <summary>
    /// Expression are true?
    /// </summary>
    /// <param name="chkUnit"> target </param>
    /// <param name="excludeCondBuff">prevent from recursion</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool Truly(Unit targetUnit = null, List<ConditionBuff> excludeCondBuff = null, Event ent = null)
    {
        if (excludeCondBuff == null)
            excludeCondBuff = new List<ConditionBuff>();
        excludeCondBuff.Add(this);
        //if target check, then check conditions by target. Else get parent
        Unit untToCheck = IsTargetCheck ? targetUnit : Parent;
        if (untToCheck == null)
            return false;

        bool res;
        //if NeedRecalc condition buff or is target check
        if (NeedRecalc || IsTargetCheck)
        {
            res = Condition.CondtionParam switch
            {
                ConditionCheckParam.SPD => CheckExpression(untToCheck.GetSpeed(null, excludeCondBuff)),
                ConditionCheckParam.CritRate => CheckExpression(untToCheck.GetCritRate(null, excludeCondBuff)),
                ConditionCheckParam.HPPrc => untToCheck.GetMaxHp(null, excludeCondBuff) != 0 &&
                                             CheckExpression(untToCheck.GetHpPrc(null, excludeCondBuff)),
                ConditionCheckParam.Weakness => untToCheck.GetWeaknesses(null, excludeCondBuff).Any(x => x == Condition.ElemValue)
                                                == (Condition.CondtionExpression == ConditionCheckExpression.Exists),
                ConditionCheckParam.Buff => untToCheck.Buffs.Any(x => x.Reference == Condition.BuffValue)
                                                == (Condition.CondtionExpression == ConditionCheckExpression.Exists),
                _ => throw new NotImplementedException()
            };
            NeedRecalc = false;



            //reset depended conditions/buffs if switched "truly"
            if (res != truly)
                foreach (Unit target in AffectedUnits())
                {

                    if (truly)
                        foreach (Effect eff in AppliedBuff.Effects)
                        {
                            eff.BeforeApply(ent, AppliedBuff,target);
                            eff.OnApply(ent, AppliedBuff,target);
                        }
                    else
                        foreach (Effect eff in AppliedBuff.Effects)
                        {
                            eff.BeforeRemove(ent, AppliedBuff,target);
                            eff.OnRemove(ent, AppliedBuff,target);
                        }
                }

            truly = res;

        }
        else
        {
            res = truly;
        }

        return res;
    }


    public record ConditionRec
    {
        public ConditionCheckExpression CondtionExpression { get; set; }
        public ConditionCheckParam CondtionParam { get; set; }
        public Unit.ElementEnm ElemValue { get; set; }

        public double Value { get; set; }
        public Buff BuffValue { get; set; }
    }
}