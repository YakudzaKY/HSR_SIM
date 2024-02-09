using System;
using HSR_SIM_LIB.Fighters;

namespace HSR_SIM_LIB.UnitStuff;

/// <summary>
///     Class for unit stats
/// </summary>
public class UnitStats:ICloneable
{
    private double? baseDef;

    public UnitStats(Unit unit)
    {
        Parent = unit;
    }

    public Unit Parent { get; set; }
    public double BaseMaxHp { get; set; } = 0;


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
                _ => (double)0
            };
        }
    }


    public double AttackFix { get; set; }

    public double AttackPrc { get; set; } = 0;

    public double BaseDef
    {
        get
        {
            baseDef ??= 200 + 10 * Parent.Level;
            return baseDef ?? 0;
        }
        set => baseDef = value;
    }


    public double DefPrc { get; set; } = 0;


    public double MaxHpFix { get; set; }

    public double MaxHpPrc { get; set; } = 0;

    public double BaseEffectRes { get; set; } = 0; //always 0 ?
    public double EffectResPrc { get; set; } = 0;


    public double BaseEffectHit { get; set; } = 0; //always 0 ?
    public double EffectHitPrc { get; set; } = 0;

    public double BaseEnergyResPrc { get; set; }

    public double BaseEnergyRes { get; set; }

    public double BaseMaxEnergy { get; set; } = 0;

    public int MaxToughness { get; set; } = 0;

    public double? LoadedBaseActionValue { get; set; }


    public double PerformedActionValue { get; set; }


    public double BaseSpeed { get; internal set; }

    public double SpeedFix { get; set; }

    public double SpeedPrc { get; set; }

    public double BaseCritChance { get; set; } = 0;
    public double CritChance => BaseCritChance + CritRatePrc;
    public double CritRatePrc { get; set; }


    public double BaseCritDmg { get; set; }

    public double CritDmg => BaseCritDmg + CritDmgPrc;
    public double CritDmgPrc { get; set; }


    public double BaseHealRate { get; set; }
    public double HealRatePrc { get; set; }

    public double HealRate => BaseHealRate + HealRatePrc;

    public void ResetAV()
    {
        PerformedActionValue = 0;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}