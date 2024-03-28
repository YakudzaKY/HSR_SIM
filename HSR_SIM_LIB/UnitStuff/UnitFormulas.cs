using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;
using HSR_SIM_LIB.Utils.Utils;

namespace HSR_SIM_LIB.UnitStuff;

public static class UnitFormulas
{
    public static Formula Attack(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseAttack)} * (1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAtkPrc).FullName} +  {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.AttackPrc)}) " +
                $"+ {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAtk).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.AttackFix)}"
        };
    }

    public static Formula EnergyRegenPrc(this Unit unit,  Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,Event ent=null, List<Condition> excludeCondition = null)
    {

        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =  $"1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffEnergyRatePrc).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseEnergyResPrc)} " +
                          $" + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseEnergyRes)} "
        };
    }

    
    public static Formula Aggro(this Unit unit,  Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker,Event ent=null, List<Condition> excludeCondition = null)
    {
        if (unit.IsAlive)
            return new Formula
            {
                EventRef = ent,
                UnitRef = unit,
                ConditionSkipList = excludeCondition,
                Expression =  $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseAggro)} * (1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffBaseAggroPrc).FullName} ) " +
                              $"* (1 +  {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAggroPrc).FullName})"
            };
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"0"
        };
    }


    
    public static Formula MaxHp(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseMaxHp)} " +
                         $" * (1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffMaxHpPrc).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxHpPrc)} )" +
                         $" + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxHpFix)} + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffMaxHp).FullName}  "
        };
    }

    public static Formula Speed(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseSpeed)} " +
                         $" * (1 + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.SpeedPrc)} " +
                         $" + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffSpeedPrc).FullName}) " +
                         $" + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.SpeedFix)} " +
                         $" + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffSpeed).FullName}"
        };
    }

    public static Formula InitialBaseActionValue(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.LoadedBaseActionValue)} ifzero (10000" +
                         $" / {unitToCheck}#{nameof(Speed)}) "
        };
    }

    public static Formula BaseActionValue(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(InitialBaseActionValue)}" +
                         $" - {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffReduceBav).FullName} "
        };
    }

    public static Formula ActionValue(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(BaseActionValue)}" +
                         $" * (1 - {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAdvance).FullName} " +
                         $" + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDelay).FullName}) "
        };
    }

    public static Formula CurrentActionValue(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            FoundedDependency = [new FormulaBuffer.DependencyRec(){Relation= unitToCheck,Stat=Condition.ConditionCheckParam.PerformedActionValue}],
            Expression = $"{unitToCheck}#{nameof(ActionValue)}" +
                         $" -  {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.PerformedActionValue)} "
        };
    }

    public static Formula HpPrc(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.GetResVal)}#{Resource.ResourceType.HP} " +
                         $" / {unitToCheck}#{nameof(MaxHp)} "
        };
    }

    public static Formula CritDamage(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.CritDmg)} " +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffCritDmg).FullName}  "
        };
    }

    public static Formula ElemBoostValue(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.GetElemBoostVal)} " +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalBoost).FullName}  "
        };
    }

    public static Formula OutgoingHealMultiplier(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"1 + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.HealRate)}" +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffOutgoingHealingPrc).FullName}  "
        };
    }

    public static Formula IncomingHealMultiplier(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffIncomeHealingPrc).FullName}  "
        };
    }

    public static Formula BreakDmg(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"1 + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BreakDmgPrc)}" +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffBreakDmgPrc).FullName}  "
        };
    }


    /// <summary>
    ///     get  abilityDamage multiplier by ability type
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="unitToCheck"></param>
    /// <param name="ent">reference to event</param>
    /// <param name="excludeCondition"></param>
    /// <returns></returns>
    public static Formula AbilityTypeMultiplier(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        if (ent is DamageEventTemplate)
            return new Formula
            {
                EventRef = ent,
                UnitRef = unit,
                ConditionSkipList = excludeCondition,
                Expression = $" 0 "
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAbilityTypeBoost).FullName}  "
        };
    }

    public static Formula EffectHit(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffEffectHitPrc).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.EffectHitPrc)} "
        };
    }

    public static Formula DebuffResists(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        var mod = (((ApplyBuff)ent)!).AppliedBuffToApply.Effects.FirstOrDefault();
        var resExpr = "";
        if (mod != null)
            resExpr = $" + {unitToCheck}#{nameof(Unit.GetDebuffResists)}#{mod.GetType().FullName}";
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDebuffResist).FullName} {resExpr} "
        };
    }

    public static Formula CcResists(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        bool isCc = (((ApplyBuff)ent)!).AppliedBuffToApply.CrowdControl;
        if (isCc)
            return new Formula
            {
                EventRef = ent,
                UnitRef = unit,
                ConditionSkipList = excludeCondition,
                Expression =
                    $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffCrowdControl).FullName} + {unitToCheck}#{nameof(Unit.GetDebuffResists)}#{typeof(EffCrowdControl).FullName}"
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $" 0 "
        };
    }

    public static Formula EffectRes(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffEffectResPrc).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.EffectResPrc)} "
        };
    }

    public static Formula DotBoost(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        if (ent is not DamageEventTemplate)
            return new Formula
            {
                EventRef = ent,
                UnitRef = unit,
                ConditionSkipList = excludeCondition,
                Expression = $" 0 "
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDoTBoost).FullName}  "
        };
    }

    public static Formula DotVulnerability(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        if (ent is not DoTDamage)
            return new Formula
            {
                EventRef = ent,
                UnitRef = unit,
                ConditionSkipList = excludeCondition,
                Expression = $"0"
            };

        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDoTVulnerability).FullName}  "
        };
    }

    public static Formula WeaknessMaxToughnessMultiplier(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxToughness)} / 120 + 0.5"
        };
    }

    public static Formula WeaknessBreakBaseDamage(Formula.DynamicTargetEnm unitToCheck, Event ent,
        Ability.ElementEnm elem, List<Condition> excludeCondition = null)
    {
        //if DoT proceed
        if (ent is ToughnessBreakDoTDamage modEnt)
        {
            string baseDmgExpr =
                $"{unitToCheck}#{nameof(Unit.UnitLvlMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(WeaknessMaxToughnessMultiplier)}";
            if (modEnt.BuffThatDamage.Effects.Any(x => x is EffBleed))
            {
                baseDmgExpr =
                    $"({OppositeTarget(unitToCheck)}#{nameof(Unit.BleedEliteMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(MaxHp)}) min (2 * {baseDmgExpr}))";
            }
            else if (modEnt.BuffThatDamage.Effects.Any(x =>
                         x is EffBurn or EffFreeze))
            {
                baseDmgExpr = "1 * " + baseDmgExpr;
            }
            else if (modEnt.BuffThatDamage.Effects.Any(x =>
                         x is EffShock))
            {
                baseDmgExpr = "2 * " + baseDmgExpr;
            }
            else if (modEnt.BuffThatDamage.Effects.Any(x =>
                         x is EffWindShear))
            {
                baseDmgExpr = $"1 * {modEnt.BuffThatDamage.Stack} * " + baseDmgExpr;
            }
            else if (modEnt.BuffThatDamage.Effects.Any(x =>
                         x is EffEntanglement))
            {
                baseDmgExpr = $"0.6 * {modEnt.BuffThatDamage.Stack} * " + baseDmgExpr;
            }
            else
            {
                throw new Exception($"{nameof(ToughnessBreakDoTDamage)} contains unknown effect");
            }

            return new Formula
            {
                EventRef = ent,
                ConditionSkipList = excludeCondition,
                Expression = baseDmgExpr
            };
        }


        //immediate weakness break
        var baseDmg = elem switch
        {
            Ability.ElementEnm.Physical => 2,
            Ability.ElementEnm.Fire => 2,
            Ability.ElementEnm.Ice => 1,
            Ability.ElementEnm.Lightning => 1,
            Ability.ElementEnm.Wind => 1.5,
            Ability.ElementEnm.Quantum => 0.5,
            Ability.ElementEnm.Imaginary => 0.5,
            _ => throw new NotImplementedException()
        };
        return new Formula
        {
            EventRef = ent,
            ConditionSkipList = excludeCondition,
            Expression =
                $"  {baseDmg} * {unitToCheck}#{nameof(Unit.UnitLvlMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(WeaknessMaxToughnessMultiplier)}"
        };
    }


    public static Formula CritHit(this Unit unit, Event ent, List<Condition> excludeCondition = null)
    {
        double critMod = ent is DirectDamage dd ? dd.IsCrit ? 1 : 0 : 0;
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{critMod}",
            FoundedDependency = [new FormulaBuffer.DependencyRec(){Relation= Formula.DynamicTargetEnm.Attacker,Stat=Condition.ConditionCheckParam.DoNotSaveDependency}]
        };
    }

    private static Formula.DynamicTargetEnm OppositeTarget(Formula.DynamicTargetEnm unitToCheck)
    {
        if (unitToCheck == Formula.DynamicTargetEnm.Attacker)
            return Formula.DynamicTargetEnm.Defender;
        return Formula.DynamicTargetEnm.Attacker;
    }

    public static Formula Def(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
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

    public static Formula DefMultiplier(Formula.DynamicTargetEnm unitToCheck, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            ConditionSkipList = excludeCondition,
            Expression =
                $"  1 - ({unitToCheck}#{nameof(Def)}  / ({unitToCheck}#{nameof(Def)} " +
                $" + 200 + (10 * {OppositeTarget(unitToCheck)}#{nameof(Unit.Level)})) ) "
        };
    }

    public static Formula ResPen(Formula.DynamicTargetEnm unitToCheck, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            ConditionSkipList = excludeCondition,
            Expression =
                $"   1 - ( ({unitToCheck}#{nameof(Unit.GetResists)} +  {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalResist).FullName} " +
                $"+  {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAllDamageResist).FullName}  )" +
                $" -  {OppositeTarget(unitToCheck)}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalPenetration).FullName} " +
                $")  "
        };
    }


    public static Formula VulnerabilityMulti(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =
                $"1 - {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalVulnerability).FullName} " +
                $"+ {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAllDamageVulnerability).FullName} " +
                $"+ {unitToCheck}#{nameof(DotVulnerability)} "
        };
    }

    public static Formula CritChance(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
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
    public static Formula GenerateCrit(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null )
    {
        var res = CritChance(unit, unitToCheck, ent, excludeCondition);
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