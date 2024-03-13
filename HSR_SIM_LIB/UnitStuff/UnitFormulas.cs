using System;
using System.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;
using HSR_SIM_LIB.Utils.Utils;

namespace HSR_SIM_LIB.UnitStuff;

public static class UnitFormulas
{
    public static Formula GetAttack(Formula.DynamicTargetEnm unitToCheck, Event ent, string prefix)
    {
        return new Formula
        {
            EventRef = ent,
            Expression =
                $"1 + {unitToCheck}#{typeof(System.Type)}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAtkPrc).FullName} + ( {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.AttackPrc)} " +
                $"* {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseAttack)})"
        };
    }

    public static Formula GetMaxHp(Formula.DynamicTargetEnm unitToCheck, Event ent)
    {
        return new Formula
        {
            EventRef = ent,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseMaxHp)} " +
                         $" * (1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffMaxHpPrc).FullName} + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxHpPrc)} )" +
                         $" + {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxHpFix)} + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffMaxHp).FullName}  "
        };
    }

    public static Formula GetCritDamage(Formula.DynamicTargetEnm unitToCheck,Event ent)
    {
        return new Formula
        {
            EventRef = ent,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.CritDmg)} " +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffCritDmg).FullName}  "
        };
    }

    public static Formula GetElemBoostValue(Formula.DynamicTargetEnm unitToCheck,Event ent = null)
    {
        return new Formula
        {
            EventRef = ent,
            Expression = $"{unitToCheck}#{nameof(Unit.GetElemBoostVal)} " +
                         
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalBoost).FullName}  "
        };
    }

    /// <summary>
    ///     get  abilityDamage multiplier by ability type
    /// </summary>
    /// <param name="ent">reference to event</param>
    /// <returns></returns>
    public static Formula GetAbilityTypeMultiplier(Formula.DynamicTargetEnm unitToCheck,Event ent)
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
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAbilityTypeBoost).FullName}  "
        };
    }

    public static Formula DotBoost(Formula.DynamicTargetEnm unitToCheck,Event ent)
    {
        if (ent is not DoTDamage)
            return new Formula
            {
                EventRef = ent,
                Expression = $" 0 "
            };

        return new Formula
        {
            EventRef = ent,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDoTBoost).FullName}  "
        };  
    }
        
    public static Formula DotVulnerability(Formula.DynamicTargetEnm unitToCheck,Event ent)
    {
        if (ent is not DoTDamage)
            return new Formula
            {
                EventRef = ent,
                Expression = $" 0 "
            };

        return new Formula
        {
            EventRef = ent,
            Expression =
                $"{unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDoTVulnerability).FullName}  "
        };  
    }
    
    public static Formula WeaknessMaxToughnessMultiplier(Formula.DynamicTargetEnm unitToCheck,Event ent = null)
    {
        return new Formula
        {
            EventRef = ent,
            Expression = $"{unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.MaxToughness)} / 120 + 0.5"
        };
    }
    
    public static Formula WeaknessBreakBaseDamage(Formula.DynamicTargetEnm unitToCheck,Event ent,Unit.ElementEnm elem)
    {

        //if DoT proceed
        if (ent is ToughnessBreakDoTDamage modEnt)
        {
            string baseDmgExpr =
                $"{unitToCheck}#{nameof(Unit.UnitLvlMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(UnitFormulas)}#{nameof(WeaknessMaxToughnessMultiplier)}";
        if (modEnt.BuffThatDamage.Effects.Any(x => x is EffBleed))
        {
            baseDmgExpr = $"({OppositeTarget(unitToCheck)}#{nameof(Unit.BleedEliteMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(UnitFormulas)}#{nameof(GetMaxHp)}) min (2 * {baseDmgExpr}))";
        }
        else if (modEnt.BuffThatDamage.Effects.Any(x =>
                     x is EffBurn or EffFreeze))
        {
            baseDmgExpr = "1 * "+baseDmgExpr;
        }
        else if (modEnt.BuffThatDamage.Effects.Any(x =>
                     x is EffShock))
        {
            baseDmgExpr = "2 * "+baseDmgExpr;
        }
        else if (modEnt.BuffThatDamage.Effects.Any(x =>
                     x is EffWindShear))
        {
            baseDmgExpr = $"1 * {modEnt.BuffThatDamage.Stack} * "+baseDmgExpr;
        }
        else if (modEnt.BuffThatDamage.Effects.Any(x =>
                     x is EffEntanglement))
        {
            baseDmgExpr = $"0.6 * {modEnt.BuffThatDamage.Stack} * "+baseDmgExpr;
        }
        else
        {
            throw new Exception($"{nameof(ToughnessBreakDoTDamage)} contains unknown effect");
        }
            return new Formula
            {
                EventRef = ent,
                Expression =baseDmgExpr
            };  
        }

        else
        {
            //immediate weakness break
            var baseDmg = elem switch
            {
                Unit.ElementEnm.Physical => 2 ,
                Unit.ElementEnm.Fire => 2 ,
                Unit.ElementEnm.Ice => 1 ,
                Unit.ElementEnm.Lightning => 1,
                Unit.ElementEnm.Wind => 1.5 ,
                Unit.ElementEnm.Quantum => 0.5 ,
                Unit.ElementEnm.Imaginary => 0.5 ,
                _ => throw new NotImplementedException()
            };
            return new Formula
            {
                EventRef = ent,
                Expression =
                    $"  {baseDmg} * {unitToCheck}#{nameof(Unit.UnitLvlMultiplier)} * {OppositeTarget(unitToCheck)}#{nameof(UnitFormulas)}#{nameof(WeaknessMaxToughnessMultiplier)}"
            };  
        }
       
        
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

    private static Formula.DynamicTargetEnm OppositeTarget(Formula.DynamicTargetEnm unitToCheck)
    {
        if (unitToCheck == Formula.DynamicTargetEnm.Attacker)
            return Formula.DynamicTargetEnm.Defender;
        return Formula.DynamicTargetEnm.Attacker;
    }
    public static Formula   GetDef(Formula.DynamicTargetEnm unitToCheck,Event ent)
    {
        return new Formula
        {
            EventRef = ent,
            Expression =
                $" {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.BaseDef)} * ( 1 + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDefPrc).FullName} " +
                //def ignore
                (ent!=null? $" - {OppositeTarget(unitToCheck)}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDefPrc).FullName} ":string.Empty )+
                $" +  {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.DefPrc)} ) + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffDef).FullName} "
        };  
      
    }
    
    public static Formula   GetDefMultiplier(Formula.DynamicTargetEnm unitToCheck,Event ent)
    {
        return new Formula
        {
            EventRef = ent,
            Expression =
                $"  1 - ({unitToCheck}#{nameof(UnitFormulas)}#{nameof(GetDef)}  / ({unitToCheck}#{nameof(UnitFormulas)}#{nameof(GetDef)} " +
                $" + 200 + (10 * {OppositeTarget(unitToCheck)}#{nameof(Unit.Level)})) ) "
        };  
      
    }
    
    public static Formula   ResPen(Formula.DynamicTargetEnm unitToCheck,Event ent)
    {
        return new Formula
        {
            EventRef = ent,
            Expression =
                $"   1 - ( ({unitToCheck}#{nameof(Unit.GetResists)} +  {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalResist).FullName} " +
                $"+  {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAllDamageResist).FullName}  )" +
                $" -  {OppositeTarget(unitToCheck)}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalPenetration).FullName} " +
                $")  "
        };  
      
    }

    
    public static Formula   VulnerabilityMulti(Formula.DynamicTargetEnm unitToCheck,Event ent)
    {
        return new Formula
        {
            EventRef = ent,
            Expression =
                $"1 - {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffElementalVulnerability).FullName} " +
                $"+ {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffAllDamageVulnerability).FullName} " +
                $"+ {unitToCheck}#{nameof(UnitFormulas)}#{nameof(DotVulnerability)} " 
        };  
      
    }

    private static Formula CritChance(Formula.DynamicTargetEnm unitToCheck,Event ent)
    {
        return new Formula
        {
            EventRef = ent,
            Expression = $" {unitToCheck}#{nameof(Unit.Stats)}#{nameof(UnitStats.CritChance)} " +
                         $"  + {unitToCheck}#{nameof(Unit.GetBuffSumByType)}#{typeof(EffCritPrc).FullName}  "
        };
    }

    /// <summary>
    ///     Crit generating. Save crit rate and crit proc into event
    /// </summary>
    /// <param name="unitToCheck"></param>
    /// <param name="ent"></param>
    /// <returns></returns>
    public static Formula GenerateCrit(Formula.DynamicTargetEnm unitToCheck,Event ent)
    {
        var res = CritChance(unitToCheck,ent);
        ((DirectDamage)ent).CritRate = res.Result;
        ((DirectDamage)ent).IsCrit = ent.ParentStep.Parent.Parent.DevMode
            ? DevModeUtils.IsCrit(ent)
            : new MersenneTwister().NextDouble() <= res.Result;

        return res;
    }
}