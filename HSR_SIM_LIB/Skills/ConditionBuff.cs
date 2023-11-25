using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class ConditionBuff : PassiveBuff
{
    public enum ConditionCheckExpression
    {
        EqualOrMore,
        EqualOrLess
    }


    public enum ConditionCheckParam
    {
        SPD,
        Weakness,
        CritRate,
        HPPrc
    }


    public ConditionBuff(Unit parentUnit) : base(parentUnit)
    {
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

    /// <summary>
    /// Expression are true?
    /// </summary>
    /// <param name="chkUnit"> target </param>
    /// <param name="excludeCondBuff">prevent from recursion</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool Truly(Unit targetUnit = null,List<ConditionBuff> excludeCondBuff=null)
    {
        if (excludeCondBuff == null)
            excludeCondBuff = new List<ConditionBuff>();
        excludeCondBuff.Add(this);
        if (Condition.ConditionAvailable != null) return Condition.ConditionAvailable();
        //if target check, then check conditions by target. Else get parent
        Unit untToCheck = IsTargetCheck ? targetUnit : Parent;
        if (untToCheck == null)
            return false;

        var res = Condition.CondtionParam switch
        {
            ConditionCheckParam.SPD => CheckExpression(untToCheck.GetSpeed(null,excludeCondBuff)),
            ConditionCheckParam.CritRate => CheckExpression(untToCheck.GetCritRate(null,excludeCondBuff)),
            ConditionCheckParam.HPPrc => untToCheck.GetMaxHp(null,excludeCondBuff) != 0 &&
                                         CheckExpression(untToCheck.GetHpPrc(null,excludeCondBuff)),
            ConditionCheckParam.Weakness => untToCheck.Fighter.Weaknesses.Any(x => x == Condition.ElemValue),
            _ => throw new NotImplementedException()
        };
        return res;
    }


    public record ConditionRec
    {
        public Ability.DCanUsePrc ConditionAvailable; //or this
        public ConditionCheckExpression CondtionExpression;
        public ConditionCheckParam CondtionParam; //us this 2 fileds
        public Unit.ElementEnm ElemValue;

        public double Value;
    }
}