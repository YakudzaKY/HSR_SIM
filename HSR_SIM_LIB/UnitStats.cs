using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{/// <summary>
/// Class for unit stats
/// </summary>
    public class UnitStats
    {
        public Unit Parent { get; set; }
        public double BaseMaxHp { get; set; } = 0;

        public double BreakDmg => (BaseBreakDmg * (1 + Parent.GetModsByType(Mod.ModifierType.BreakDmgPrc) + BreakDmgPrc)) + Parent.GetModsByType(Mod.ModifierType.BreakDmg);
        public double BreakDmgPrc { get; set; } = 0;

        public double BaseBreakDmg { get; set; } = 0;
        public double BaseAttack { get; set; } = 0;


        public double Attack => (BaseAttack * (1 + Parent.GetModsByType(Mod.ModifierType.AtkPrc) + AttackPrc)) + Parent.GetModsByType(Mod.ModifierType.Atk) + AttackFix;
        public double AttackFix { get; set; }

        public double AttackPrc { get; set; } = 0;

        private double? baseDef;
        public double BaseDef
        {
            get
            {
                if (baseDef == null)
                    baseDef = 200 + (10 * Parent.Level);
                return baseDef ?? 0;
            }
            set => baseDef = value;
        }

        public double Def => (BaseDef * (1 + Parent.GetModsByType(Mod.ModifierType.DefPrc) + DefPrc)) + Parent.GetModsByType(Mod.ModifierType.Def);
        public double DefPrc { get; set; } = 0;


        public double MaxHp => (BaseMaxHp * (1 + Parent.GetModsByType(Mod.ModifierType.MaxHpPrc) + MaxHpPrc)) + Parent.GetModsByType(Mod.ModifierType.MaxHp) + MaxHpFix;
        public double MaxHpFix { get; set; }

        public double MaxHpPrc { get; set; } = 0;

        public double BaseEffectRes { get; set; } = 0;
        public double EffectResPrc { get; set; } = 0;
        public double EffectResFix { get; set; } = 0;
        public double EffectRes => (BaseEffectRes * (1 + Parent.GetModsByType(Mod.ModifierType.EffectResPrc) + EffectResPrc)) + Parent.GetModsByType(Mod.ModifierType.EffectRes) + EffectResFix;

        public double BaseEffectHit { get; set; } = 0;
        public double EffectHitPrc { get; set; } = 0;
        public double EffectHitFix { get; set; } = 0;
        public double EffectHit => (BaseEffectHit * (1 + Parent.GetModsByType(Mod.ModifierType.EffectHitPrc) + EffectHitPrc)) + Parent.GetModsByType(Mod.ModifierType.EffectHit) + EffectHitFix;



        public int CurrentEnergy { get; set; } = 0;
        public int BaseMaxEnergy { get; set; } = 0;

        public int MaxToughness { get; set; } = 0;

        private double? baseActionValue;
        public double BaseActionValue
        {
            get => baseActionValue ?? 10000 / Speed;
            set => baseActionValue = value;
        }

        public double BaseCritChance { get; set; } = 0;
        public double CritChance => BaseCritChance + Parent.GetModsByType(Mod.ModifierType.CritPrc) + CritRatePrc;
        public double CritRatePrc { get; set; }

        public double ActionValue { get; set; } = 0;
        public double BaseSpeed { get; internal set; }
        public double Speed => (BaseSpeed * (1 + Parent.GetModsByType(Mod.ModifierType.SpeedPrc) + SpeedPrc)) + Parent.GetModsByType(Mod.ModifierType.Speed) + SpeedFix;
        public double SpeedFix { get; set; }

        public double SpeedPrc { get; set; }

        public double BaseCritDmg { get; set; }

        public double CritDmg => BaseCritDmg + Parent.GetModsByType(Mod.ModifierType.CritDmg) + CritDmgPrc;
        public double CritDmgPrc { get; set; }

        public UnitStats(Unit unit)
        {
            Parent=unit;
        }

        public void ResetAV()
        {
            ActionValue = BaseActionValue;
        }
    }
}
