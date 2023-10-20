using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills
{
    public class Effect
    {
        public Event.CalculateValuePrc CalculateValue { get; init; }
        public EffectType EffType { get; set; }
        public double? Value { get; set; }
        public bool StackAffectValue { get; set; } = true;// do we multiply final value by stack count ?
        public List<Ability.AbilityTypeEnm> AbilityTypes { get; set; } = new List<Ability.AbilityTypeEnm>();//for ability type amplification
        public Unit.ElementEnm? Element { get; init; }
        public enum EffectType
        {
            AtkPrc,
            Atk,
            DefPrc,
            Def,
            MaxHpPrc,
            MaxHp,
            BreakDmgPrc,
            SpeedPrc,
            Speed,
            CritPrc,
            CritDmg,
            EffectResPrc,
            EffectRes,
            EffectHitPrc,
            EffectHit,
            ElementalBoost,
            AllDamageBoost,
            ElementalPenetration,
            DoTBoost,
            DamageReduction,
            AllDamageVulnerability,
            ElementalVulnerability,
            DoTVulnerability,
            ElementalResist,
            DebufResist,
            AbilityTypeBoost,
            IncomeHealingPrc,
            EnergyRatePrc,
            Bleed,
            Imprisonment,
            Delay,
            Entanglement,
            Burn,
            Frozen,
            Shock,
            WindShear,
            Dominated,
            Outrage,
            CrowControl,
            ReduceSpdPrc
        }
    }
}
