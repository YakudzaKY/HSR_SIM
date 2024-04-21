using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.LightCones;

internal class BeforetheTutorialMissionStarts : DefaultLightCone
{
    private readonly double[] modifiersEnrg =
        { 4, 5, 6, 7, 8 };

    public BeforetheTutorialMissionStarts(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path == Parent.Path)
            //add event to energy regen to all attacks
            foreach (var ability in Parent.Abilities.Where(x =>
                         x.Attack && x.AbilityType != Ability.AbilityTypeEnm.Technique))
            {
                EnergyGain enrgEvent = new(null, this, Parent.Parent)
                {
                    TargetUnit = Parent.Parent,
                    CalculateValue = CalcEnergyRgn
                };

                ability.Events.Add(enrgEvent);
            }
    }


    public sealed override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Nihility;

    private double? CalcEnergyRgn(Event ent)
    {
        //if hit any target that have def debuff or def ignore by player
        var targetHits = (from p in ent.ParentStep.Events
                where p is DirectDamage &&
                      (p.TargetUnit.GetBuffSumByType(ent, typeof(EffDef), buffType: Buff.BuffType.Debuff) < 0
                       || p.TargetUnit.GetBuffSumByType(ent, typeof(EffDefPrc), buffType: Buff.BuffType.Debuff) < 0
                       || p.SourceUnit.GetBuffSumByType(ent, typeof(EffDefIgnore)) > 0)
                select p.TargetUnit)
            .Distinct();
        var emmTargets = targetHits as Unit[] ?? targetHits.ToArray();
        return emmTargets.Length > 0 ? modifiersEnrg[Rank - 1] : 0;
    }
}