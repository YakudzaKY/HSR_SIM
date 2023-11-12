using System;
using System.Linq;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class ConditionMod : PassiveMod
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


    public ConditionMod(Unit parentUnit) : base(parentUnit)
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

    public bool Truly(Unit chkUnit = null)
    {
        if (Condition.ConditionAvailable != null) return Condition.ConditionAvailable();

        switch (chkUnit)
        {
            case null when IsTargetCheck:
                return false;
            case null:
                chkUnit = Parent;
                break;
        }

        var res = Condition.CondtionParam switch
        {
            ConditionCheckParam.SPD => CheckExpression(chkUnit.GetSpeed(null)),
            ConditionCheckParam.CritRate => CheckExpression(chkUnit.GetCritRate(null)),
            ConditionCheckParam.HPPrc => chkUnit.GetMaxHp(null) != 0 &&
                                         CheckExpression(chkUnit.GetHpPrc(null)),
            ConditionCheckParam.Weakness => chkUnit.Fighter.Weaknesses.Any(x => x == Condition.ElemValue),
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