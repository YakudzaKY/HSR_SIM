using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones;

internal class BeforetheTutorialMissionStarts : DefaultLightCone
{
    
    private readonly Dictionary<int, double> modifiersEnrg = new()
        { { 1, 4 }, { 2, 5}, { 3, 6 }, { 4, 7 }, { 5, 8 } };
    public BeforetheTutorialMissionStarts(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path == Parent.Path)
        {
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
        //if hit any target that have def debuff or def ignore by player
        var targetHits = (from p in ent.ParentStep.Events 
                where (p  is DirectDamage) &&(p.TargetUnit.GetModsByType(typeof(EffDef),buffType:Buff.BuffType.Debuff)<0 
                                              ||p.TargetUnit.GetModsByType(typeof(EffDefPrc),buffType:Buff.BuffType.Debuff)<0 
                                              ||p.SourceUnit.DefIgnore(ent)>0 )
                select p.TargetUnit)
            .Distinct();
        var emmTargets = targetHits as Unit[] ?? targetHits.ToArray();
        return emmTargets.Length>0? modifiersEnrg[Rank]:0 ;
    }


    public sealed override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Nihility;
}