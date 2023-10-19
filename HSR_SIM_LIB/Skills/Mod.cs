using System;
using System.Collections.Generic;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills
{
    public class Mod : CloneClass
    {
        public delegate double? CalculateValuePrc(Event ent);
        public delegate IEnumerable<Unit> CalculateTargetPrc();

        public CalculateValuePrc CalculateValue { get; init; }
        public CalculateTargetPrc CalculateTargets { get; init; }
        public ModType Type { get; init; }
        public Unit TargetUnit { get; set; }
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
            Debuff
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
            IncomeHealingPrc
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

            return string.Format(">> {0:s} {1:s} for {2:s} val= {3:D} duration={4:D} dispellable={5:s}",
                Type.ToString()
                , TargetUnit.Name
                , modsStr
                , Value.ToString()
                , BaseDuration.ToString()
                , Dispellable.ToString()
                );
        }
    }
}
