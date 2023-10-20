using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills
{
    public class Mod : CloneClass
    {
     
        //public delegate IEnumerable<Unit> CalculateTargetPrc();

        public Event.CalculateValuePrc CalculateValue { get; init; }
       // public CalculateTargetPrc CalculateTargets { get; init; }
        public ModType Type { get; init; }
       // public Unit TargetUnit { get; set; }
        public string CustomIconName { get; set; }
        public List<ModifierType> Modifiers { get; init; }=new List<ModifierType>();
        public double? Value { get; set; }
        public int Stack { get; set; } = 1;   
        public int MaxStack { get; set; } = 1;
        public int? BaseDuration { get; set; }
        public int? DurationLeft { get; set; }
        public List<Ability.AbilityTypeEnm> AbilityTypes { get; set; } = new List<Ability.AbilityTypeEnm>();//for ability type amplification
        public string UniqueStr { get; set; }

        public Mod RefMod { get; set; }

        public bool Dispellable { get; init; }

        public Unit.ElementEnm? Element { get; init; }


        public enum ModType
        {
            Buff,
            Debuff,
            Dot
        }

        
        public enum ModifierType
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
            AbilityTypeBoost,
            IncomeHealingPrc,
            EnergyRatePrc
        }

        public Mod(Mod reference)
        {
            RefMod = reference;
        }
        public string GetDescription()
        {

            string modsStr="";
            foreach (var mod in Modifiers)
            {
                modsStr += mod.ToString()+"; ";
            }

            return
                $">> {Type.ToString():s} for {modsStr:s} val= {Value.ToString():D} duration={BaseDuration.ToString():D} dispellable={Dispellable.ToString():s}";
        }
    }
}
