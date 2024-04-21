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

public static class UnitStaticFormulas
{
    public static Formula.DynamicTargetEnm OppositeTarget(Formula.DynamicTargetEnm unitToCheck)
    {
        if (unitToCheck == Formula.DynamicTargetEnm.Attacker)
            return Formula.DynamicTargetEnm.Defender;
        return Formula.DynamicTargetEnm.Attacker;
    }
    
    public static Formula WeaknessBreakBaseDamage(Formula.DynamicTargetEnm unitToCheck, Event ent,
        Ability.ElementEnm elem, List<Condition> excludeCondition = null)
    {
        //if DoT proceed
        if (ent is ToughnessBreakDoTDamage modEnt)
        {
            string baseDmgExpr =
                $"{unitToCheck}#{nameof(Unit.UnitLvlMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(Unit.WeaknessMaxToughnessMultiplier)}";
            if (modEnt.BuffThatDamage.Effects.Any(x => x is EffBleed))
            {
                baseDmgExpr =
                    $"({OppositeTarget(unitToCheck)}#{nameof(Unit.BleedEliteMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(Unit.MaxHp)}) min (2 * {baseDmgExpr}))";
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
            _ => throw new Exception("Weakness break unknown element "+elem)
        };
        return new Formula
        {
            EventRef = ent,
            ConditionSkipList = excludeCondition,
            Expression =
                $"  {baseDmg} * {unitToCheck}#{nameof(Unit.UnitLvlMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(Unit.WeaknessMaxToughnessMultiplier)}"
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
                $"  1 - ({unitToCheck}#{nameof(Unit.Def)}  / ({unitToCheck}#{nameof(Unit.Def)} " +
                $" + 200 + (10 * {OppositeTarget(unitToCheck)}#{nameof(Unit.Level)})) ) "
        };
    }
    
    public static Formula ResPen(Formula.DynamicTargetEnm unitToCheck,Ability.ElementEnm elem, Event ent = null,
        List<Condition> excludeCondition = null)
    {
        return new Formula
        {
            EventRef = ent,
            ConditionSkipList = excludeCondition,
            Expression =
                $" 1 - ( {unitToCheck}#{nameof(Unit.Resists)}" +
                $" - {OppositeTarget(unitToCheck)}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalPenetration).FullName}#{elem.ToString()} " +
                $") "
        };
    }
}