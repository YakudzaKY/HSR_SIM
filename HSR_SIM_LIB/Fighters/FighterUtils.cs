using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils.Utils;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.Fighters;

/// <summary>
///     ultils
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
        MainDPS,
        SecondDPS,
        ThirdDPS,
        Support,
        Healer
    }

    private static readonly Dictionary<int, double> lvlMultiplier;

    /// <summary>
    ///     get from table in https://honkai-star-rail.fandom.com/wiki/Toughness#Weakness_Break
    /// </summary>
    static FighterUtils()
    {
        lvlMultiplier = new Dictionary<int, double>
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
    }

    /// <summary>
    ///     Calc dmg when shield broken
    ///     https://honkai-star-rail.fandom.com/wiki/Toughness#Weakness_Break
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="caster"></param>
    /// <returns></returns>
    public static double? CalculateShieldBrokeDmg(Event ent)
    {
        if (!(ent is ToughnessBreak) && !(ent is ToughnessBreakDoTDamage)) return 0;

        var attacker = ent.SourceUnit;
        var defender = ent.TargetUnit;
        var attackElem = ent.ParentStep.ActorAbility?.Element ?? attacker.Fighter.Element;

        ent.ParentStep.Parent.Parent?.LogDebug("=======================");
        ent.ParentStep.Parent.Parent?.LogDebug(
            $"{attacker.Name:s} ({attacker.ParentTeam.Units.IndexOf(attacker) + 1:d}) Break shield of {defender.Name} ({defender.ParentTeam.Units.IndexOf(defender) + 1:d})");
        double baseDmg;
        var maxToughnessMult = 0.5 + (double)defender.Stats.MaxToughness / 120;
        //if this is direct shield break
        if (ent is ToughnessBreak)
        {
            baseDmg = attackElem switch
            {
                ElementEnm.Physical => 2 * lvlMultiplier[attacker.Level] * maxToughnessMult,
                ElementEnm.Fire => 2 * lvlMultiplier[attacker.Level] * maxToughnessMult,
                ElementEnm.Ice => 1 * lvlMultiplier[attacker.Level] * maxToughnessMult,
                ElementEnm.Lightning => 1 * lvlMultiplier[attacker.Level] * maxToughnessMult,
                ElementEnm.Wind => 1.5 * lvlMultiplier[attacker.Level] * maxToughnessMult,
                ElementEnm.Quantum => 0.5 * lvlMultiplier[attacker.Level] * maxToughnessMult,
                ElementEnm.Imaginary => 0.5 * lvlMultiplier[attacker.Level] * maxToughnessMult,
                _ => throw new NotImplementedException()
            };
        }
        else
        {
            var modEnt = ent as ToughnessBreakDoTDamage;
            if (modEnt.Modification.Effects.Any(x => x is EffBleed))
            {
                var eliteFlag = defender.Fighter is DefaultNPCBossFIghter;
                baseDmg = eliteFlag ? 0.07 : 0.16 * defender.GetMaxHp(ent);
                baseDmg = Math.Min(baseDmg, 2 * lvlMultiplier[attacker.Level] * defender.Stats.MaxToughness);
            }
            else if (modEnt.Modification.Effects.Any(x =>
                         x is EffBurn or EffFreeze))
            {
                baseDmg = 1 * lvlMultiplier[attacker.Level] * maxToughnessMult;
            }
            else if (modEnt.Modification.Effects.Any(x =>
                         x is EffShock))
            {
                baseDmg = 2 * lvlMultiplier[attacker.Level] * maxToughnessMult;
            }
            else if (modEnt.Modification.Effects.Any(x =>
                         x is EffWindShear))
            {
                baseDmg = 1 * modEnt.Modification.Stack * lvlMultiplier[attacker.Level] * maxToughnessMult;
            }
            else if (modEnt.Modification.Effects.Any(x =>
                         x is EffEntanglement))
            {
                baseDmg = 0.6 * modEnt.Modification.Stack * lvlMultiplier[attacker.Level] * maxToughnessMult;
            }
            else
            {
                baseDmg = 0;
            }
        }

        var breakEffect = 1 + attacker.GetBreakDmg(ent);
        var def = defender.GetDef(ent);
        var defWithIgnore = def * (1 - attacker.DefIgnore(ent));
        var defMultiplier = 1 - defWithIgnore / (defWithIgnore + 200 + 10 * attacker.Level);
        var resPen = 1 - (defender.GetResists(attackElem, ent) - attacker.ResistsPenetration(attackElem, ent));
        var vulnMult = 1 + defender.GetElemVulnerability(attackElem, ent) + defender.GetAllDamageVulnerability(ent);
        var brokenMultiplier = defender.GetBrokenMultiplier();
        var totalDmg = baseDmg * breakEffect * defMultiplier * resPen * vulnMult * brokenMultiplier;
        ent.ParentStep.Parent.Parent?.LogDebug($"baseDmg({baseDmg:f}) ; breakEffect({breakEffect:f})");
        ent.ParentStep.Parent.Parent?.LogDebug($"Def {def:f} -> ignored to {defWithIgnore:f} ");
        ent.ParentStep.Parent.Parent?.LogDebug(
            $"defMultiplier({defMultiplier:f}) = 1-(defender.Stats.Def({defWithIgnore:f})/(defender.Stats.Def({defWithIgnore:f})+200+(10*attacker.Level({attacker.Level:d}))))");
        ent.ParentStep.Parent.Parent?.LogDebug(
            $"resPen= {resPen:f} ; vulnMult= {vulnMult:f} ; brokenMultiplier= {brokenMultiplier:f}");
        ent.ParentStep.Parent.Parent?.LogDebug($"TOTAL DAMAGE= {totalDmg:f}");
        return totalDmg;
    }

    /// <summary>
    ///     calc real value by stats
    ///     https://honkai-star-rail.fandom.com/wiki/Damage
    /// </summary>
    /// <param name="baseDmg"></param>
    /// <param name="ent"></param>
    /// <returns></returns>
    public static double CalculateDmgByBasicVal(double baseDmg, Event ent)
    {
        var attacker = ent.SourceUnit;
        var defender = ent.TargetUnit;

        var attackElem = ent.ParentStep.ActorAbility.Element;

        //crit multiplier
        double critMultiplier = 1;
        double dotMultiplier = 0;
        double dotVulnerability = 0;
        if (ent is DirectDamage)
        {
            ((DirectDamage)ent).IsCrit = new MersenneTwister().NextDouble() <= attacker.GetCritRate(ent);
            if (((DirectDamage)ent).IsCrit)
                critMultiplier = 1 + attacker.GetCritDamage(ent);
        }
        else
        {
            dotMultiplier = attacker.DotBoost(ent);
            dotVulnerability = defender.GetDotVulnerability(ent);
        }


        var damageBoost = 1
                          + attacker.GetElemBoostValue(attackElem, ent)
                          + attacker.GetAbilityTypeMultiplier(ent)
                          + attacker.AllDmgBoost(ent)
                          + dotMultiplier
            ;

        var def = defender.GetDef(ent);
        var defWithIgnore = def * (1 - attacker.DefIgnore(ent));
        var defMultiplier = 1 - defWithIgnore / (defWithIgnore + 200 + 10 * attacker.Level);

        var resPen = 1 - (defender.GetResists(attackElem, ent) - attacker.ResistsPenetration(attackElem, ent));

        var vulnMult = 1 + defender.GetElemVulnerability(attackElem, ent) + defender.GetAllDamageVulnerability(ent) +
                       dotVulnerability;

        var dmgReduction = defender.GetDamageReduction(defender,ent);

        var brokenMultiplier = defender.GetBrokenMultiplier();
        var totalDmg = baseDmg * critMultiplier * damageBoost * defMultiplier * resPen * vulnMult *
                       dmgReduction * brokenMultiplier;
        ent.ParentStep.Parent.Parent?.LogDebug("=======================");
        ent.ParentStep.Parent.Parent?.LogDebug($"baseDmg={baseDmg:f} crit chance={attacker.GetCritRate(ent):f} ");
        ent.ParentStep.Parent.Parent?.LogDebug(
            $"{attacker.Name:s} ({attacker.ParentTeam.Units.IndexOf(attacker) + 1:d}) Damaging {defender.Name} ({defender.ParentTeam.Units.IndexOf(defender) + 1:d})");
        ent.ParentStep.Parent.Parent?.LogDebug(
            $"damageBoost({damageBoost:f}) = 1+ GetElemBoostValue({attacker.GetElemBoostValue(attackElem, ent):f})  +AllDmgBoost({attacker.AllDmgBoost(ent):f}) + dotMultiplier({dotMultiplier:f})");
        ent.ParentStep.Parent.Parent?.LogDebug($"Def {def:f} -> ignored to {defWithIgnore:f} ");
        ent.ParentStep.Parent.Parent?.LogDebug(
            $"defMultiplier({defMultiplier:f}) = 1-(defender.Stats.Def({defWithIgnore:f})/(defender.Stats.Def({defWithIgnore:f})+200+(10*attacker.Level({attacker.Level:d}))))");
        ent.ParentStep.Parent.Parent?.LogDebug(
            $"resPen= {resPen:f} ; vulnMult= {vulnMult:f} ; critMultiplier={critMultiplier:f} ; dmgReduction= {dmgReduction:f} ; brokenMultiplier= {brokenMultiplier:f} ; abilityTypeMultiplier {attacker.GetAbilityTypeMultiplier(ent):f} dmg proportion={((DamageEventTemplate)ent).CalculateProportion:f}");
        ent.ParentStep.Parent.Parent?.LogDebug($"TOTAL DAMAGE= {totalDmg:f}");
        return totalDmg;
    }


    //debuff is resisted?
    public static bool CalculateDebuffApplied(ApplyBuff ent,double baseChance)
    {

        var attacker = ent.SourceUnit;
        var defender = ent.TargetUnit;
        var mod = ent.BuffToApply.Effects.FirstOrDefault();
        var isCC = ent.BuffToApply.CrowdControl;
        var effectHitRate = attacker.GetEffectHit(ent);
        var effectRes = defender.GetEffectRes(ent);
        var debuffRes = defender.GetDebuffResists(mod?.GetType()??typeof(Effect), ent);
        double ccRes = 0;
        if (isCC) ccRes = defender.GetDebuffResists(typeof(EffCrowControl));
        var realChance = baseChance * (1 + effectHitRate) * (1 - effectRes) * (1 - debuffRes) * (1 - ccRes);


        ent.ParentStep.Parent.Parent?.LogDebug("=======================");
        ent.ParentStep.Parent.Parent?.LogDebug(
            $"Debuff realChance {realChance:f} =baseChance {baseChance:f} * (1+ effectHitRate {effectHitRate:f})* (1- effectRes {effectRes:f})  * (1- debuffRes {debuffRes:f})" +
            (isCC ? $"* (1- ccRes {ccRes:f})" : ""));


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

        ent.ParentStep.Parent.Parent?.LogDebug("=======================");
        ent.ParentStep.Parent.Parent?.LogDebug(
            $"Shield val is {shieldVal:f} = (baseVal {baseVal:f} + additiveShieldBonus {additiveShieldBonus:f}) * (1+ prcShieldBonus{prcShieldBonus:f})");
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
        ent.ParentStep.Parent.Parent?.LogDebug("=======================");
        ent.ParentStep.Parent.Parent?.LogDebug(
            $"Heal val is {res:f} = baseHeal{baseHeal:f} * outMod{outMod:f} *  inMod{inMod:f}");
        return res;
    }
}