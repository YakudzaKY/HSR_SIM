using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using static HSR_SIM_LIB.Skills.Effect;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.UnitStuff
{/// <summary>
/// Class for unit stats
/// </summary>
    public class UnitStats
    {
        public Unit Parent { get; set; }
        public double BaseMaxHp { get; set; } = 0;

        public double BreakDmg => Parent.GetModsByType(EffectType.BreakDmgPrc) + BreakDmgPrc;
        public double BreakDmgPrc { get; set; } = 0;

        public double BaseAttack { get; set; } = 0;

        //base aggro chance by https://honkai-star-rail.fandom.com/wiki/Aggro?so=search
        public double BaseAggro
        {
            get
            {
                if (!Parent.IsAlive)
                    return 0;
                return Parent.Fighter.Path switch
                {
                    FighterUtils.PathType.Hunt => 3,
                    FighterUtils.PathType.Erudition => 3,
                    FighterUtils.PathType.Harmony => 4,
                    FighterUtils.PathType.Nihility => 4,
                    FighterUtils.PathType.Abundance => 4,
                    FighterUtils.PathType.Destruction => 5,
                    FighterUtils.PathType.Preservation => 6,
                    _ => (double)0,
                };
            }
        }
        public double Aggro => BaseAggro * (1 + Parent.GetModsByType(EffectType.BaseAgrroPrc) ) * (1+Parent.GetModsByType(EffectType.AgrroPrc));
        public double Attack => BaseAttack * (1 + Parent.GetModsByType(EffectType.AtkPrc) + AttackPrc) + Parent.GetModsByType(EffectType.Atk) + AttackFix;
        public double AttackFix { get; set; }

        public double AttackPrc { get; set; } = 0;

        private double? baseDef;
        public double BaseDef
        {
            get
            {
                baseDef ??= 200 + 10 * Parent.Level;
                return baseDef ?? 0;
            }
            set => baseDef = value;
        }

        public double Def => BaseDef * (1 + Parent.GetModsByType(EffectType.DefPrc) + DefPrc) + Parent.GetModsByType(EffectType.Def);
        public double DefPrc { get; set; } = 0;


        public double MaxHp => BaseMaxHp * (1 + Parent.GetModsByType(EffectType.MaxHpPrc) + MaxHpPrc) + Parent.GetModsByType(EffectType.MaxHp) + MaxHpFix;
        public double MaxHpFix { get; set; }

        public double MaxHpPrc { get; set; } = 0;

        public double BaseEffectRes { get; set; } = 0;//always 0 ?
        public double EffectResPrc { get; set; } = 0;
        public double EffectRes =>   Parent.GetModsByType(EffectType.EffectResPrc) + EffectResPrc+BaseEffectRes + Parent.GetModsByType(EffectType.EffectRes) ;

        public double BaseEffectHit { get; set; } = 0;//always 0 ?
        public double EffectHitPrc { get; set; } = 0;
        public double EffectHit =>   Parent.GetModsByType(EffectType.EffectHitPrc) + EffectHitPrc + BaseEffectHit + Parent.GetModsByType(EffectType.EffectHit) ;

        public double EnergyRegenPrc => BaseEnergyRes * (1 + Parent.GetModsByType(EffectType.EnergyRatePrc) + BaseEnergyResPrc) ;
        public double BaseEnergyResPrc { get; set; }

        public double BaseEnergyRes { get; set; }

        public double BaseMaxEnergy { get; set; } = 0;

        public int MaxToughness { get; set; } = 0;

        private double? baseActionValue;
        public double InitialBaseActionValue
        {
            get => baseActionValue ?? 10000 / Speed;
            set => baseActionValue = value;
        }

        public double BaseActionValue
        {
            get => InitialBaseActionValue - Parent.GetModsByType(EffectType.ReduceBAV);

        }


        public double PerformedActionValue { get; set; } = 0;

        public double ActionValue
        {
            get
            {
                return BaseActionValue*(1- Parent.GetModsByType(EffectType.Advance) + Parent.GetModsByType(EffectType.Delay)) - PerformedActionValue;
            }

        } 
        public double BaseSpeed { get; internal set; }
        public double Speed => BaseSpeed * (1 + Parent.GetModsByType(EffectType.SpeedPrc) -Parent.GetModsByType(EffectType.ReduceSpdPrc) + SpeedPrc) + Parent.GetModsByType(EffectType.Speed) + SpeedFix;
        public double SpeedFix { get; set; }

        public double SpeedPrc { get; set; }

        public double BaseCritChance { get; set; } = 0;
        public double CritChance => BaseCritChance  + CritRatePrc;
        public double CritRatePrc { get; set; }



        public double BaseCritDmg { get; set; }

        public double CritDmg => BaseCritDmg + CritDmgPrc;
        public double CritDmgPrc { get; set; }

        public double CurrentEnergy
        {
            get => Parent.GetRes(Resource.ResourceType.HP).ResVal;
            set => Parent.GetRes(Resource.ResourceType.HP).ResVal = value;
        }

        public double BaseHealRate { get; set; }
        public double HealRatePrc { get; set; }

        public double HealRate => BaseHealRate  + HealRatePrc;

        public UnitStats(Unit unit)
        {
            Parent = unit;
        }

        public void ResetAV()
        {
            PerformedActionValue = 0;
        }
    }
}
