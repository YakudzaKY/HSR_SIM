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
    private readonly Dictionary<int, double> modifiersEnrg = new()
        { { 1, 3 }, { 2, 3.5}, { 3, 4 }, { 4, 4.5 }, { 5, 5 } };

    private readonly Dictionary<int, double> modifiersSpd = new()
        { { 1, 12 }, { 2, 14}, { 3, 16 }, { 4, 18 }, { 5, 20 } };
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
                        Type = Buff.BuffType.Buff, Effects = new List<Effect> { new EffSpeed() { Value = modifiersSpd[Rank] } },
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
        var targetHits = (from p in ent.ParentStep.Events 
                where p  is DirectDamage
                select p.TargetUnit)
            .Distinct();
        var emmTargets = targetHits as Unit[] ?? targetHits.ToArray();
        return modifiersEnrg[Rank] * Math.Min(emmTargets.Count(), 3);
    }



    public override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Abundance;
}