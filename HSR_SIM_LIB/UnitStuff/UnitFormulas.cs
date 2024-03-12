using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;
using HSR_SIM_LIB.Utils.Utils;

namespace HSR_SIM_LIB.UnitStuff;

public static class UnitFormulas
{
    public static Formula GetAttack(Event ent, string prefix)
    {
        return new Formula
        {
            EventRef = ent,
            Expression =
                $"1 + {Formula.DynamicTargetEnm.Attacker}#{typeof(System.Type)}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAtkPrc).FullName} + ( {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Stats)}#{nameof(UnitStats.AttackPrc)} " +
                $"* {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseAttack)})"
        };
    }

    public static Formula GetMaxHp(Event ent)
    {
        return new Formula
        {
            EventRef = ent,
            Expression = $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseMaxHp)} " +
                         $" * (1 + {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffMaxHpPrc).FullName} + {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxHpPrc)} )" +
                         $" + {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxHpFix)} + {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffMaxHp).FullName}  "
        };
    }

    public static Formula GetCritDamage(Event ent)
    {
        return new Formula
        {
            EventRef = ent,
            Expression = $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Stats)}#{nameof(UnitStats.CritDmg)} " +
                         $"  + {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffCritDmg).FullName}  "
        };
    }

    public static Formula GetElemBoostValue(Event ent = null)
    {
        return new Formula
        {
            EventRef = ent,
            Expression = $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.GetElemBoostVal)} " +
                         $"  + {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalBoost).FullName}  "
        };
    }

    /// <summary>
    ///     get  abilityDamage multiplier by ability type
    /// </summary>
    /// <param name="ent">reference to event</param>
    /// <param name="ability">reference to ability</param>
    /// <returns></returns>
    public static Formula GetAbilityTypeMultiplier(Event ent, Ability ability)
    {
        if (ent is DoTDamage)
            return new Formula
            {
                EventRef = ent,
                Expression = $" 0 "
            };

        return new Formula
        {
            EventRef = ent,
            Expression =
                $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAbilityTypeBoost).FullName}  "
        };
    }

    public static Formula CritHit(Event ent)
    {
        double critMod = ((DirectDamage)ent).IsCrit ? 1 : 0;
        return new Formula
        {
            EventRef = ent,
            Expression = $"{critMod}"
        };
    }


    public static Formula CritChance(Event ent)
    {
        return new Formula
        {
            EventRef = ent,
            Expression = $" {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Stats)}#{nameof(UnitStats.CritChance)} " +
                         $"  + {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffCritPrc).FullName}  "
        };
    }

    /// <summary>
    ///     Crit generating. Save crit rate and crit proc into event
    /// </summary>
    /// <param name="ent"></param>
    /// <returns></returns>
    public static Formula GenerateCrit(Event ent)
    {
        var res = CritChance(ent);
        ((DirectDamage)ent).CritRate = res.Result;
        ((DirectDamage)ent).IsCrit = ent.ParentStep.Parent.Parent.DevMode
            ? DevModeUtils.IsCrit(ent)
            : new MersenneTwister().NextDouble() <= res.Result;

        return res;
    }
}