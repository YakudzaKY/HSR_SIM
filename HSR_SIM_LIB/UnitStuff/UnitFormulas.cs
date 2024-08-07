﻿using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;
using HSR_SIM_LIB.Utils.Utils;
using static HSR_SIM_LIB.UnitStuff.UnitStaticFormulas;

namespace HSR_SIM_LIB.UnitStuff;

public partial class Unit : CloneClass
{
    public Formula Attack(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.BaseAttack)} * (1 + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffAtkPrc).FullName} +  {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.AttackPrc)}) " +
                $"+ {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffAtk).FullName} + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.AttackFix)}"
        };
    }

    public Formula EnergyRegenPrc(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,
        Event ent = null, List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"1 + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffEnergyRatePrc).FullName} + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.BaseEnergyResPrc)} " +
                $" + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.BaseEnergyRes)} "
        };
    }


    public Formula Aggro(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        if (IsAlive)
            return new Formula
            {
                EventRef = ent,
                UnitRef = this,
                ConditionSkipList = excludeCondition,
                Expression =
                    $"{unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.BaseAggro)} * (1 + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffBaseAggroPrc).FullName} ) " +
                    $"* (1 +  {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffAggroPrc).FullName})"
            };
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = "0"
        };
    }


    public Formula MaxHp(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.BaseMaxHp)} " +
                         $" * (1 + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffMaxHpPrc).FullName} + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.MaxHpPrc)} )" +
                         $" + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.MaxHpFix)} + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffMaxHp).FullName}  "
        };
    }

    public Formula Speed(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.BaseSpeed)} " +
                         $" * (1 + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.SpeedPrc)} " +
                         $" + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffSpeedPrc).FullName}) " +
                         $" + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.SpeedFix)} " +
                         $" + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffSpeed).FullName}"
        };
    }

    public Formula InitialBaseActionValue(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.LoadedBaseActionValue)} ifzero (10000" +
                         $" / {unitToCheck}#{nameof(Speed)}) "
        };
    }

    public Formula BaseActionValue(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(InitialBaseActionValue)}" +
                         $" - {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffReduceBav).FullName} "
        };
    }

    public Formula ActionValue(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,
        Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(BaseActionValue)}" +
                         $" * (1 - {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffAdvance).FullName} " +
                         $" + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffDelay).FullName}) "
        };
    }

    public Formula CurrentActionValue(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,
        Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            FoundedDependency =
            [
                new FormulaBuffer.DependencyRec
                    { Relation = unitToCheck, Stat = Condition.ConditionCheckParam.PerformedActionValue }
            ],
            Expression = $"{unitToCheck}#{nameof(ActionValue)}" +
                         $" -  {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.PerformedActionValue)} "
        };
    }

    public Formula HpPrc(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(GetResVal)}#{Resource.ResourceType.HP} " +
                         $" / {unitToCheck}#{nameof(MaxHp)} "
        };
    }

    public Formula CritDamage(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,
        Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.CritDmg)} " +
                         $"  + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffCritDmg).FullName}  "
        };
    }

    public Formula ElemBoostValue(Ability.ElementEnm elem,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(GetBaseElemBoostVal)}#{elem.ToString()}  " +
                         $"  + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffElementalBoost).FullName}#{elem.ToString()}   "
        };
    }

    public Formula OutgoingHealMultiplier(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,
        Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"1 + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.HealRate)}" +
                         $"  + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffOutgoingHealingPrc).FullName}  "
        };
    }

    public Formula IncomingHealMultiplier(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,
        Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"1 + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffIncomeHealingPrc).FullName}  "
        };
    }

    public Formula BreakDmg(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"1 + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.BreakDmgPrc)}" +
                         $"  + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffBreakDmgPrc).FullName}  "
        };
    }


    /// <summary>
    ///     get  abilityDamage multiplier by ability type
    /// </summary>
    /// <param name="abilityType"></param>
    /// <param name="unitToCheck"></param>
    /// <param name="ent">reference to event</param>
    /// <param name="excludeCondition"></param>
    /// <returns></returns>
    public Formula AbilityTypeMultiplier(Ability.AbilityTypeEnm abilityType,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        if (ent is not  DirectDamage)
            return new Formula
            {
                EventRef = ent,
                UnitRef = this,
                ConditionSkipList = excludeCondition,
                Expression = " 0 "
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffAbilityTypeBoost).FullName}#{abilityType.ToString()}  "
        };
    }

    public Formula EffectHit(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffEffectHitPrc).FullName} + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.EffectHitPrc)} "
        };
    }

    public Formula DebuffResists(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,
        Event ent = null, Effect effect = null,
        List<Condition> excludeCondition = null)
    {
        var resExpr = "";
        if (effect != null)
            resExpr = $" + {unitToCheck}#{nameof(GetNativeDebuffResists)}#{effect.GetType().FullName}";
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffDebuffResist).FullName} {resExpr} "
        };
    }

    public Formula CcResists(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        var isCc = ((ApplyBuff)ent)!.AppliedBuffToApply.CrowdControl;
        if (isCc)
            return new Formula
            {
                EventRef = ent,
                UnitRef = this,
                ConditionSkipList = excludeCondition,
                Expression =
                    $"{unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffCrowdControl).FullName} + {unitToCheck}#{nameof(GetNativeDebuffResists)}#{typeof(EffCrowdControl).FullName}"
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = " 0 "
        };
    }

    public Formula EffectRes(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffEffectResPrc).FullName} + {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.EffectResPrc)} "
        };
    }

    public Formula DotBoost(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        if (ent is not DamageEventTemplate)
            return new Formula
            {
                EventRef = ent,
                UnitRef = this,
                ConditionSkipList = excludeCondition,
                Expression = " 0 "
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffDoTBoost).FullName}  "
        };
    }

    public Formula DotVulnerability(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,
        Event ent = null,
        List<Condition> excludeCondition = null)
    {
        if (ent is not DoTDamage)
            return new Formula
            {
                EventRef = ent,
                UnitRef = this,
                ConditionSkipList = excludeCondition,
                Expression = "0"
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffDoTVulnerability).FullName}  "
        };
    }

    public Formula WeaknessMaxToughnessMultiplier(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.MaxToughness)} / 120 + 0.5"
        };
    }





    public Formula Def(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $" {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.BaseDef)} * ( 1 + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffDefPrc).FullName} " +
                //def ignore
                (ent != null
                    ? $" - {OppositeTarget(unitToCheck)}#{nameof(GetBuffSumByType)}#{typeof(EffDefIgnore).FullName} "
                    : string.Empty) +
                $" +  {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.DefPrc)} ) + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffDef).FullName} "
        };
    }


    public Formula Resists(Ability.ElementEnm elem,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $" {unitToCheck}#{nameof(GetNativeResists)}#{elem.ToString()} +  {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffElementalResist).FullName}#{elem.ToString()}  " +
                $" +  {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffAllDamageResist).FullName} "
        };
    }


    public Formula VulnerabilityMulti(Ability.ElementEnm elem,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"1 - {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffElementalVulnerability).FullName}#{elem.ToString()}  " +
                $"+ {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffAllDamageVulnerability).FullName} " +
                $"+ {unitToCheck}#{nameof(DotVulnerability)} "
        };
    }

    public Formula CritChance(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $" {unitToCheck}#{nameof(Stats)}#{nameof(UnitStats.CritChance)} " +
                         $"  + {unitToCheck}#{nameof(GetBuffSumByType)}#{typeof(EffCritPrc).FullName}  "
        };
    }

    /// <summary>
    ///     Crit generating. Save crit rate and crit proc into event
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="unitToCheck"></param>
    /// <param name="ent"></param>
    /// <param name="excludeCondition"></param>
    /// <returns></returns>
    public Formula GenerateCrit(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        var res = CritChance(unitToCheck, ent, excludeCondition);
        if (ent is DirectDamage dd)
        {
            dd.CritRate = res.Result;
            dd.IsCrit = ent.ParentStep.Parent.Parent.DevMode
                ? DevModeUtils.IsCrit(ent)
                : new MersenneTwister().NextDouble() <= res.Result;
            FormulaBuffer.MergeDependency(res.FoundedDependency,
                new FormulaBuffer.DependencyRec
                    { Stat = Condition.ConditionCheckParam.DoNotSaveDependency, Relation = unitToCheck });
        }

        return res;
    }
}