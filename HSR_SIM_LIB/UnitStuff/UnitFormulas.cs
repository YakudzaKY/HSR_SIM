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
    public static Formula GetAttack(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =
                $"1 + {unitToCheck}#{typeof(System.Type)}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAtkPrc).FullName} + ( {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.AttackPrc)} " +
                $"* {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseAttack)})"
        };
    }

    public static Formula GetMaxHp(this Unit unit,
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

    public static Formula GetSpeed(this Unit unit,
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

    public static Formula GetInitialBaseActionValue(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.LoadedBaseActionValue)} ifzero 10000" +
                         $" / {unitToCheck}#{nameof(UnitFormulas)}#{nameof(GetSpeed)} "
        };
    }

    public static Formula GetBaseActionValue(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(UnitFormulas)}#{nameof(GetInitialBaseActionValue)}" +
                         $" - {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffReduceBAV).FullName} "
        };
    }

    public static Formula GetActionValue(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(UnitFormulas)}#{nameof(GetBaseActionValue)}" +
                         $" * (1 - {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAdvance).FullName}) " +
                         $" + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDelay).FullName}) "
        };
    }

    public static Formula GetCurrentActionValue(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(UnitFormulas)}#{nameof(GetActionValue)}" +
                         $" -  {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.PerformedActionValue)} "
        };
    }

    public static Formula GetHpPrc(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{unitToCheck}#{nameof(Unit.GetResVal)}#{Resource.ResourceType.HP} " +
                         $" / {unitToCheck}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetMaxHp)} "
        };
    }

    public static Formula GetCritDamage(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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

    public static Formula GetElemBoostValue(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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

    public static Formula GetOutgoingHealMultiplier(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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

    public static Formula GetIncomingHealMultiplier(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffIncomeHealingPrc).FullName}  "
        };
    }

    public static Formula GetBreakDmg(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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
    /// <param name="ent">reference to event</param>
    /// <returns></returns>
    public static Formula GetAbilityTypeMultiplier(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDebufResist).FullName} {resExpr} "
        };
    }

    public static Formula CcResists(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
    {
        bool isCc = (((ApplyBuff)ent)!).AppliedBuffToApply.CrowdControl;
        if (isCc)
            return new Formula
            {
                EventRef = ent,
                UnitRef = unit,
                ConditionSkipList = excludeCondition,
                Expression =
                    $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffCrowControl).FullName} + {unitToCheck}#{nameof(Unit.GetDebuffResists)}#{typeof(EffCrowControl).FullName}"
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
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
    {
        if (ent is not DoTDamage)
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
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDoTVulnerability).FullName}  "
        };
    }

    public static Formula WeaknessMaxToughnessMultiplier(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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
                $"{unitToCheck}#{nameof(Unit.UnitLvlMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(UnitFormulas)}#{nameof(WeaknessMaxToughnessMultiplier)}";
            if (modEnt.BuffThatDamage.Effects.Any(x => x is EffBleed))
            {
                baseDmgExpr =
                    $"({OppositeTarget(unitToCheck)}#{nameof(Unit.BleedEliteMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(UnitFormulas)}#{nameof(GetMaxHp)}) min (2 * {baseDmgExpr}))";
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
                    $"  {baseDmg} * {unitToCheck}#{nameof(Unit.UnitLvlMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(UnitFormulas)}#{nameof(WeaknessMaxToughnessMultiplier)}"
            };
        
    }


    public static Formula CritHit(this Unit unit, Event ent, List<Condition> excludeCondition = null)
    {
        double critMod = ((DirectDamage)ent).IsCrit ? 1 : 0;
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression = $"{critMod}"
        };
    }

    private static Formula.DynamicTargetEnm OppositeTarget(Formula.DynamicTargetEnm unitToCheck)
    {
        if (unitToCheck == Formula.DynamicTargetEnm.Attacker)
            return Formula.DynamicTargetEnm.Defender;
        return Formula.DynamicTargetEnm.Attacker;
    }

    public static Formula GetDef(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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

    public static Formula GetDefMultiplier(Formula.DynamicTargetEnm unitToCheck, Event ent = null, List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            ConditionSkipList = excludeCondition,
            Expression =
                $"  1 - ({unitToCheck}#{nameof(UnitFormulas)}#{nameof(GetDef)}  / ({unitToCheck}#{nameof(UnitFormulas)}#{nameof(GetDef)} " +
                $" + 200 + (10 * {OppositeTarget(unitToCheck)}#{nameof(Unit.Level)})) ) "
        };
    }

    public static Formula ResPen(Formula.DynamicTargetEnm unitToCheck, Event ent = null, List<Condition> excludeCondition = null)
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
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            UnitRef = unit,
            ConditionSkipList = excludeCondition,
            Expression =
                $"1 - {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalVulnerability).FullName} " +
                $"+ {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAllDamageVulnerability).FullName} " +
                $"+ {unitToCheck}#{nameof(UnitFormulas)}#{nameof(DotVulnerability)} "
        };
    }

    public static Formula CritChance(this Unit unit,
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
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
        Formula.DynamicTargetEnm unitToCheck = Formula.DynamicTargetEnm.Attacker, Event ent = null, List<Condition> excludeCondition = null)
    {
        var res = CritChance(unit, unitToCheck, ent,excludeCondition);
        (((DirectDamage)ent)!).CritRate = res.Result;
        ((DirectDamage)ent).IsCrit = ent.ParentStep.Parent.Parent.DevMode
            ? DevModeUtils.IsCrit(ent)
            : new MersenneTwister().NextDouble() <= res.Result;

        return res;
    }
}