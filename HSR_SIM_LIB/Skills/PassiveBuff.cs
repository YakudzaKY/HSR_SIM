using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_LIB.Skills;

public class PassiveBuff : Buff
{
    public enum ConditionCheckExpression
    {
        EqualOrMore,
        EqualOrLess,
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
        AnyDebuff
    }

    private bool truly;

    public PassiveBuff(Unit parent) : base(parent,null)
    {
        CarrierUnit = parent;
    }

    public object Target { get; set; } //in most cases target==parent, but when target is full team then not
    public bool IsTargetCheck { get; init; }


    public ConditionRec Condition { get; init; }
    public bool NeedRecalc { get; set; } = true;

    /// <summary>
    ///     get list of affected targets
    /// </summary>
    /// <returns></returns>
    private Unit[] AffectedUnits()
    {
        Unit[] affectedTargets;
        if (Target is Unit utr)
            affectedTargets = new[] { utr };
        else if (Target is Team tm)
            affectedTargets = tm.Units.Where(x => x.IsAlive).ToArray();
        else if (Target is TargetTypeEnm tte)
            affectedTargets = CarrierUnit.GetTargetsForUnit(tte).ToArray();
        else
            affectedTargets = new Unit[] { };
        return affectedTargets;
    }

    /// <summary>
    ///     unit is affected by this
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool UnitIsAffected(Unit unit)
    {
        if ((Target is Unit && Target == unit)
            || (Target is Team && Target == unit.ParentTeam)
            || (Target is TargetTypeEnm &&
                CarrierUnit.GetTargetsForUnit((TargetTypeEnm)Target).Contains(unit)))
            return true;
        return false;
    }


    private bool CheckExpression(double targetVal)
    {
        if (Condition.ConditionExpression == ConditionCheckExpression.EqualOrMore)
            return targetVal >= Condition.Value;
        if (Condition.ConditionExpression == ConditionCheckExpression.EqualOrLess)
            return targetVal <= Condition.Value;
        throw new NotImplementedException();
    }

    /// <summary>
    ///     get list of unit affected by condition buff
    /// </summary>
    /// <returns></returns>
    /// <summary>
    ///     Expression are true?
    /// </summary>
    /// <param name="targetUnit"></param>
    /// <param name="excludeCondBuff">prevent from recursion</param>
    /// <param name="ent"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool Truly(Unit targetUnit = null, List<PassiveBuff> excludeCondBuff = null, Event ent = null)
    {
        if (Condition == null) return true;
        if (excludeCondBuff == null)
            excludeCondBuff = new List<PassiveBuff>();
        excludeCondBuff.Add(this);
        //if target check, then check conditions by target. Else get parent
        var untToCheck = IsTargetCheck ? ent?.TargetUnit : targetUnit ?? CarrierUnit;
        if (untToCheck == null)
            return false;

        bool res;
        //if NeedRecalc condition buff or is target check
        if (NeedRecalc || IsTargetCheck)
        {
            res = Condition.ConditionParam switch
            {
                ConditionCheckParam.Spd => CheckExpression(untToCheck.GetSpeed(ent, excludeCondBuff)),
                ConditionCheckParam.CritRate => CheckExpression(untToCheck.GetCritRate(ent, excludeCondBuff)),
                ConditionCheckParam.HpPrc => untToCheck.GetMaxHp(ent, excludeCondBuff) != 0 &&
                                             CheckExpression(untToCheck.GetHpPrc(ent, excludeCondBuff)),
                ConditionCheckParam.Weakness => untToCheck.GetWeaknesses(ent, excludeCondBuff)
                                                    .Any(x => x == Condition.ElemValue)
                                                == (Condition.ConditionExpression == ConditionCheckExpression.Exists),
                ConditionCheckParam.Buff => untToCheck.AppliedBuffs.Any(x => x.Reference == Condition.AppliedBuffValue)
                                            == (Condition.ConditionExpression == ConditionCheckExpression.Exists),
                ConditionCheckParam.AnyDebuff => untToCheck.AppliedBuffs.Any(x =>
                                                     x.Type is AppliedBuff.BuffType.Debuff
                                                         or AppliedBuff.BuffType.Dot)
                                                 == (Condition.ConditionExpression == ConditionCheckExpression.Exists),
                _ => throw new NotImplementedException()
            };
            NeedRecalc = false;


            //reset depended conditions/buffs if switched "truly"
            if (res != truly)
                foreach (var target in AffectedUnits())
                    if (truly)
                        foreach (var eff in Effects)
                        {
                            eff.BeforeApply(ent, this, target);
                            eff.OnApply(ent, this, target);
                        }
                    else
                        foreach (var eff in Effects)
                        {
                            eff.BeforeRemove(ent, this, target);
                            eff.OnRemove(ent, this, target);
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
        public ConditionCheckExpression ConditionExpression { get; init; }
        public ConditionCheckParam ConditionParam { get; init; }
        public Unit.ElementEnm ElemValue { get; init; }

        public double Value { get; init; }
        public AppliedBuff AppliedBuffValue { get; init; }
    }
}