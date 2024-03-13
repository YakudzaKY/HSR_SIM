using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;
using HSR_SIM_LIB.Utils.Utils;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.Content;

/// <summary>
///     Combat utilities
/// </summary>
public static class FighterUtils
{
    public enum PathType
    {
        Destruction,
        Hunt,
        Erudition,
        Harmony,
        Nihility,
        Preservation,
        Abundance
    }

    public enum UnitRole
    {
        MainDps,
        SecondDps,
        ThirdDps,
        Support,
        Healer
    }
    /// <summary>
    ///     get from table in https://honkai-star-rail.fandom.com/wiki/Toughness#Weakness_Break
    ///     if someone know formula plz rewrite
    /// </summary>

    public static readonly Dictionary<int, double> LvlMultiplier= new()
    {
        { 1, 54.0000 }, { 2, 58.0000 }, { 3, 62.0000 }, { 4, 67.5264 }, { 5, 70.5094 }, { 6, 73.5228 },
        { 7, 76.5660 }, { 8, 79.6385 }, { 9, 82.7395 }, { 10, 85.8684 }, { 11, 91.4944 }, { 12, 97.0680 },
        { 13, 102.5892 }, { 14, 108.0579 }, { 15, 113.4743 }, { 16, 118.8383 }, { 17, 124.1499 }, { 18, 129.4091 },
        { 19, 134.6159 }, { 20, 139.7703 }, { 21, 149.3323 }, { 22, 158.8011 }, { 23, 168.1768 }, { 24, 177.4594 },
        { 25, 186.6489 }, { 26, 195.7452 }, { 27, 204.7484 }, { 28, 213.6585 }, { 29, 222.4754 }, { 30, 231.1992 },
        { 31, 246.4276 }, { 32, 261.1810 }, { 33, 275.4733 }, { 34, 289.3179 }, { 35, 302.7275 }, { 36, 315.7144 },
        { 37, 328.2905 }, { 38, 340.4671 }, { 39, 352.2554 }, { 40, 363.6658 }, { 41, 408.1240 }, { 42, 451.7883 },
        { 43, 494.6798 }, { 44, 536.8188 }, { 45, 578.2249 }, { 46, 618.9172 }, { 47, 658.9138 }, { 48, 698.2325 },
        { 49, 736.8905 }, { 50, 774.9041 }, { 51, 871.0599 }, { 52, 964.8705 }, { 53, 1056.4206 },
        { 54, 1145.7910 }, { 55, 1233.0585 }, { 56, 1318.2965 }, { 57, 1401.5750 }, { 58, 1482.9608 },
        { 59, 1562.5178 }, { 60, 1640.3068 }, { 61, 1752.3215 }, { 62, 1861.9011 }, { 63, 1969.1242 },
        { 64, 2074.0659 }, { 65, 2176.7983 }, { 66, 2277.3904 }, { 67, 2375.9085 }, { 68, 2472.4160 },
        { 69, 2566.9739 }, { 70, 2659.6406 }, { 71, 2780.3044 }, { 72, 2898.6022 }, { 73, 3014.6029 },
        { 74, 3128.3729 }, { 75, 3239.9758 }, { 76, 3349.4730 }, { 77, 3456.9236 }, { 78, 3562.3843 },
        { 79, 3665.9099 }, { 80, 3767.5533 }
    };


    
    
    /// <summary>
    ///     Calc dmg when shield broken
    ///     https://honkai-star-rail.fandom.com/wiki/Toughness#Weakness_Break
    /// </summary>
    /// <param name="ent"></param>
    /// <returns></returns>

    public static Formula WeaknessBreakFormula()
    {
    
        var expression =
                        //base damage
                         $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.WeaknessBreakBaseDamage)} " +
                         //breakEffect
                         $" * (1 + {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffBreakDmgPrc).FullName} + {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Stats)}#{nameof(UnitStats.BreakDmgPrc)} ) " +
                         //def
                         $" * ({Formula.DynamicTargetEnm.Defender}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetDefMultiplier)}) "  +
                         //resist and  penetration
                         $" *  {Formula.DynamicTargetEnm.Defender}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.ResPen)}  " +
                         //vulnerability 
                         $" *  {Formula.DynamicTargetEnm.Defender}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.VulnerabilityMulti)} " +
                         //broken multiplier
                         $" *  {Formula.DynamicTargetEnm.Defender}#{nameof(Unit.GetBrokenMultiplier)}";
                       
        var newFormula = new Formula { Expression = expression};

        return newFormula;
    }
    
    /// <summary>
    ///     Generate damage output formula
    /// </summary>
    /// <param name="abilityFormula">formula to take ability damage</param>
    /// <returns></returns>
    public static Formula DamageFormula(Formula abilityFormula)
    {
        var expression = $"{abilityFormula.Expression} * " +
                         //crit
                         $" ( ({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GenerateCrit)} * 0) " +
                         $" + ({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetCritDamage)} * {Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.CritHit)} )  + 1 ) " +
                         //damage boost
                         $" * (1 + {Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetElemBoostValue)} " +
                         $" + {Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetAbilityTypeMultiplier)} " +
                         $" + {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAllDamageBoost).FullName} " +
                         $" + {Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.DotBoost)}  ) " +
                         //def
                         $" * ({Formula.DynamicTargetEnm.Defender}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetDefMultiplier)}) "  +
                         //resist and  penetration
                         $" *  {Formula.DynamicTargetEnm.Defender}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.ResPen)}  " +
                         //vulnerability 
                         $" *  {Formula.DynamicTargetEnm.Defender}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.VulnerabilityMulti)} " +
                         //damage reduction
                         $" * {Formula.DynamicTargetEnm.Defender}#{nameof(Unit.GetBuffMultiplyByType)}#{typeof(EffDamageReduction).FullName} " +
                         //broken multiplier
                         $" *  {Formula.DynamicTargetEnm.Defender}#{nameof(Unit.GetBrokenMultiplier)}";
        var newFormula = new Formula { Expression = expression, Variables = abilityFormula.Variables };

        return newFormula;
    }

    


    //debuff is resisted?
    public static bool CalculateDebuffApplied(ApplyBuff ent, double baseChance)
    {
        if (ent.ParentStep.Parent.Parent.DevMode)
            return DevModeUtils.IsDebuffed(ent);
        var attacker = ent.SourceUnit;
        var defender = ent.TargetUnit;
        var mod = ent.AppliedBuffToApply.Effects.FirstOrDefault();
        var isCC = ent.AppliedBuffToApply.CrowdControl;
        var effectHitRate = attacker.GetEffectHit(ent);
        var effectRes = defender.GetEffectRes(ent);
        var debuffRes = defender.GetDebuffResists(mod?.GetType() ?? typeof(Effect), ent);
        double ccRes = 0;
        if (isCC) ccRes = defender.GetDebuffResists(typeof(EffCrowControl));
        var realChance = baseChance * (1 + effectHitRate) * (1 - effectRes) * (1 - debuffRes) * (1 - ccRes);

        return new MersenneTwister().NextDouble() <= realChance;
    }

    /// <summary>
    /// </summary>
    /// <param name="baseDmg"></param>
    /// <param name="ent"></param>
    /// <returns></returns>
    public static double CalculateShield(double baseVal, Event ent, Unit caster = null)
    {
        var attacker = caster ?? ent.SourceUnit;


        var additiveShieldBonus = attacker.AdditiveShieldBonus(ent);
        var prcShieldBonus = attacker.PrcShieldBonus(ent);
        var shieldVal = (baseVal + additiveShieldBonus) * (1 + prcShieldBonus);


        return shieldVal;
    }

    /// <summary>
    ///     Calc healing numbers
    /// </summary>
    /// <param name="baseHeal"></param>
    /// <param name="ent"></param>
    /// <returns></returns>
    public static double CalculateHealByBasicVal(double baseHeal, Event ent)
    {
        var healer = ent.SourceUnit;
        var receiver = ent.TargetUnit;
        var outMod = healer.GetOutgoingHealMultiplier(ent);
        var inMod = receiver.GetIncomingHealMultiplier(ent);
        var res = baseHeal * outMod * inMod;

        return res;
    }

    /// <summary>
    ///     Get Skill Modifier By min, max and level
    /// </summary>
    /// <param name="modAt1">skill mod at 1 lvl</param>
    /// <param name="modAt10">skill mod at 10 lvl </param>
    /// <param name="level">level can be higher than 10</param>
    /// <returns></returns>
    public static double GetAbilityScaling(double modAt1, double modAt10, int level)
    {
        var maxLvl = 10; //without Eidolons
        var firstModLvl = 6;
        if (level <= firstModLvl) return modAt1 + (modAt10 - modAt1) * (level - 1) / maxLvl;
        if (level <= maxLvl)
        {
            //first get val from 6 lvl
            var preMod = GetAbilityScaling(modAt1, modAt10, firstModLvl);
            return preMod + (modAt10 - preMod) * 1 / (maxLvl - firstModLvl) * (level - firstModLvl);
        }

        return modAt10 + (modAt10 - modAt1) * 1 / maxLvl * (level - maxLvl);
    }

    /// <summary>
    ///     Get Basic ability Modifier By min, max and level
    /// </summary>
    /// <param name="modAt1">skill mod at 1 lvl</param>
    /// <param name="modAt6">skill mod at 6 lvl </param>
    /// <param name="level">level can be higher than 10</param>
    /// <returns></returns>
    public static double GetBasicScaling(double modAt1, double modAt6, int level)
    {
        var maxLvl = 6; //without Eidolons

        return modAt1 + (level - 1) / (maxLvl - 1) * (modAt6 - modAt6);
    }
}