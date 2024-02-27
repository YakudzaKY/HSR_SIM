using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_CONTENT.Content.LightCones;

internal class EchoesoftheCoffin : DefaultLightCone
{
    private readonly double[] modifiersEnrg =
        { 3, 3.5, 4, 4.5, 5 };

    private readonly double[] modifiersSpd =
        { 12, 14, 16, 18, 20 };

    public EchoesoftheCoffin(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path == Parent.Path)
        {
            //add event buff to ultimate
            foreach (var ability in Parent.Abilities.Where(x => x.AbilityType == Ability.AbilityTypeEnm.Ultimate))
            {
                ApplyBuff eventBuff = new(null, this, Parent.Parent)
                {
                    CalculateTargets = ((DefaultFighter)Parent).GetAliveFriends,
                    AppliedBuffToApply = new AppliedBuff(Parent.Parent)
                    {
                        Type = Buff.BuffType.Buff,
                        Effects = new List<Effect> { new EffSpeed { Value = modifiersSpd[Rank - 1] } },
                        BaseDuration = 1
                    }
                };
                ability.Events.Add(eventBuff);
            }

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
    }


    public override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Abundance;


    private double? CalcEnergyRgn(Event ent)
    {
        return modifiersEnrg[Rank - 1] * Math.Min(ent.ParentStep.TargetsHit.Count(), 3);
    }
}