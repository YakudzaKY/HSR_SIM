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
                switch (Parent.Fighter.Path)
                {
                    case FighterUtils.PathType.Hunt:
                        return 3;
                    case FighterUtils.PathType.Erudition:
                        return 3;
                    case FighterUtils.PathType.Harmony:
                        return 4;
                    case FighterUtils.PathType.Nihility:
                        return 4;
                    case FighterUtils.PathType.Abundance:
                        return 4;
                    case FighterUtils.PathType.Destruction:
                        return 5;
                    case FighterUtils.PathType.Preservation:
                        return 6;
                    default:
                        return 0;
                }
            
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
                if (baseDef == null)
                    baseDef = 200 + 10 * Parent.Level;
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
        public double BaseActionValue
        {
            get => baseActionValue ?? 10000 / Speed;
            set => baseActionValue = value;
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
        public double CritChance => BaseCritChance + Parent.GetModsByType(EffectType.CritPrc) + CritRatePrc;
        public double CritRatePrc { get; set; }



        public double BaseCritDmg { get; set; }

        public double CritDmg => BaseCritDmg + Parent.GetModsByType(EffectType.CritDmg) + CritDmgPrc;
        public double CritDmgPrc { get; set; }

        public double CurrentEnergy
        {
            get => Parent.GetRes(Resource.ResourceType.HP).ResVal;
            set => Parent.GetRes(Resource.ResourceType.HP).ResVal = value;
        }

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
