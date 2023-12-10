using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones;

internal class EchoesoftheCoffin : DefaultLightCone
{
    private readonly double[] modifiersEnrg = 
        { 3 ,  3.5,  4 , 4.5 ,  5  };

    private readonly double[] modifiersSpd = 
        {  12 ,  14,  16 , 18 ,  20  };
    public EchoesoftheCoffin(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path == Parent.Path)
        {
            //add event buff to ultimate
            foreach (Ability ability in Parent.Abilities.Where(x => x.AbilityType == Ability.AbilityTypeEnm.Ultimate))
            {
                ApplyBuff eventBuff = new(null, this, Parent.Parent)
                {   CalculateTargets = ((DefaultFighter)Parent).GetAliveFriends,
                    AbilityValue = ability,
                    BuffToApply = new Buff(Parent.Parent)
                    {
                        Type = Buff.BuffType.Buff, Effects = new List<Effect> { new EffSpeed() { Value = modifiersSpd[Rank-1] } },
                        BaseDuration = 1
                    }
                };
                ability.Events.Add(eventBuff);
            }
            //add event to energy regen to all attacks
            foreach (Ability ability in Parent.Abilities.Where(x => x.Attack && x.AbilityType!=Ability.AbilityTypeEnm.Technique))
            {
                EnergyGain enrgEvent = new(null, this, Parent.Parent)
                {
                    TargetUnit = Parent.Parent,
                    CalculateValue = CalcEnergyRgn
                };
             
                ability.Events.Add(enrgEvent);
            }
        }
    }


    private double? CalcEnergyRgn(Event ent)
    {
        return modifiersEnrg[Rank-1] * Math.Min(ent.ParentStep.TargetsHit.Count(), 3);
    }



    public override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Abundance;
}