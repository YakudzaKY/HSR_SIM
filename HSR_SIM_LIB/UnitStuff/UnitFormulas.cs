using System;
using System.Collections.Generic;
using System.Linq;
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
                $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseAttack)} * (1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAtkPrc).FullName} +  {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.AttackPrc)}) " +
                $"+ {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAtk).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.AttackFix)}"
        };
    }

    public  Formula EnergyRegenPrc(  Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,Event ent=null, List<Condition> excludeCondition = null)
    {

        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =  $"1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffEnergyRatePrc).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseEnergyResPrc)} " +
                          $" + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseEnergyRes)} "
        };
    }

    
    public  Formula Aggro( Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,Event ent=null, List<Condition> excludeCondition = null)
    {
        if (IsAlive)
            return new Formula
            {
                EventRef = ent,
                UnitRef = this,
                ConditionSkipList = excludeCondition,
                Expression =  $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseAggro)} * (1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffBaseAggroPrc).FullName} ) " +
                              $"* (1 +  {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAggroPrc).FullName})"
            };
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"0"
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
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseMaxHp)} " +
                         $" * (1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffMaxHpPrc).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxHpPrc)} )" +
                         $" + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxHpFix)} + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffMaxHp).FullName}  "
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
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseSpeed)} " +
                         $" * (1 + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.SpeedPrc)} " +
                         $" + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffSpeedPrc).FullName}) " +
                         $" + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.SpeedFix)} " +
                         $" + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffSpeed).FullName}"
        };
    }

    public  Formula InitialBaseActionValue(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.LoadedBaseActionValue)} ifzero (10000" +
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
                         $" - {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffReduceBav).FullName} "
        };
    }

    public Formula ActionValue(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(BaseActionValue)}" +
                         $" * (1 - {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAdvance).FullName} " +
                         $" + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDelay).FullName}) "
        };
    }

    public Formula CurrentActionValue(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            FoundedDependency = [new FormulaBuffer.DependencyRec(){Relation= unitToCheck,Stat=Condition.ConditionCheckParam.PerformedActionValue}],
            Expression = $"{unitToCheck}#{nameof(ActionValue)}" +
                         $" -  {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.PerformedActionValue)} "
        };
    }

    public  Formula HpPrc(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.GetResVal)}#{Resource.ResourceType.HP} " +
                         $" / {unitToCheck}#{nameof(MaxHp)} "
        };
    }

    public Formula CritDamage(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.CritDmg)} " +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffCritDmg).FullName}  "
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
            Expression = $"{unitToCheck}#{nameof(Unit.GetBaseElemBoostVal)}#{elem.ToString()}  " +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalBoost).FullName}#{elem.ToString()}   "
        };
    }

    public  Formula OutgoingHealMultiplier(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"1 + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.HealRate)}" +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffOutgoingHealingPrc).FullName}  "
        };
    }

    public Formula IncomingHealMultiplier(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffIncomeHealingPrc).FullName}  "
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
            Expression = $"1 + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BreakDmgPrc)}" +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffBreakDmgPrc).FullName}  "
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
        if (ent is DamageEventTemplate)
            return new Formula
            {
                EventRef = ent,
                UnitRef = this,
                ConditionSkipList = excludeCondition,
                Expression = $" 0 "
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAbilityTypeBoost).FullName}#{abilityType.ToString()}  "
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
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffEffectHitPrc).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.EffectHitPrc)} "
        };
    }

    public Formula DebuffResists(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, Effect effect=null,
        List<Condition> excludeCondition = null)
    {
      
        var resExpr = "";
        if (effect != null)
            resExpr = $" + {unitToCheck}#{nameof(Unit.GetNativeDebuffResists)}#{effect.GetType().FullName}";
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDebuffResist).FullName} {resExpr} "
        };
    }

    public Formula CcResists(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        bool isCc = (((ApplyBuff)ent)!).AppliedBuffToApply.CrowdControl;
        if (isCc)
            return new Formula
            {
                EventRef = ent,
                UnitRef = this,
                ConditionSkipList = excludeCondition,
                Expression =
                    $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffCrowdControl).FullName} + {unitToCheck}#{nameof(Unit.GetNativeDebuffResists)}#{typeof(EffCrowdControl).FullName}"
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $" 0 "
        };
    }

    public  Formula EffectRes(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffEffectResPrc).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.EffectResPrc)} "
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
                Expression = $" 0 "
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDoTBoost).FullName}  "
        };
    }

    public Formula DotVulnerability(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        if (ent is not DoTDamage)
            return new Formula
            {
                EventRef = ent,
                UnitRef = this,
                ConditionSkipList = excludeCondition,
                Expression = $"0"
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDoTVulnerability).FullName}  "
        };
    }

    public Formula WeaknessMaxToughnessMultiplier(Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxToughness)} / 120 + 0.5"
        };
    }

    

    public Formula CritHit( Event ent, List<Condition> excludeCondition = null)
    {
        double critMod = ent is DirectDamage dd ? dd.IsCrit ? 1 : 0 : 0;
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $"{critMod}",
            FoundedDependency = [new FormulaBuffer.DependencyRec(){Relation= Formula.DynamicTargetEnm.Attacker,Stat=Condition.ConditionCheckParam.DoNotSaveDependency}]
        };
    }



    public  Formula Def(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $" {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseDef)} * ( 1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDefPrc).FullName} " +
                //def ignore
                (ent != null
                    ? $" - {OppositeTarget(unitToCheck)}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDefIgnore).FullName} "
                    : string.Empty) +
                $" +  {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.DefPrc)} ) + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDef).FullName} "
        };
    }



    public  Formula Resists(Ability.ElementEnm elem,Formula.DynamicTargetEnm unitToCheck= Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $" {unitToCheck}#{nameof(Unit.GetNativeResists)}#{elem.ToString()} +  {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalResist).FullName}#{elem.ToString()}  " +
                $" +  {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAllDamageResist).FullName} "
        };
    }



    public  Formula VulnerabilityMulti(Ability.ElementEnm elem,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression =
                $"1 - {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalVulnerability).FullName}#{elem.ToString()}  " +
                $"+ {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAllDamageVulnerability).FullName} " +
                $"+ {unitToCheck}#{nameof(DotVulnerability)} "
        };
    }

    public  Formula CritChance(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = this,
            ConditionSkipList = excludeCondition,
            Expression = $" {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.CritChance)} " +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffCritPrc).FullName}  "
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
    public  Formula GenerateCrit(
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null )
    {
        var res = CritChance(unitToCheck, ent, excludeCondition);
        if (ent is DirectDamage dd)
        {
            dd.CritRate = res.Result;
            dd.IsCrit = ent.ParentStep.Parent.Parent.DevMode
                ? DevModeUtils.IsCrit(ent)
                : new MersenneTwister().NextDouble() <= res.Result;
            FormulaBuffer.MergeDependency(res.FoundedDependency, new FormulaBuffer.DependencyRec(){Stat = Condition.ConditionCheckParam.DoNotSaveDependency,Relation =unitToCheck });
        }

        return res;
    }
}