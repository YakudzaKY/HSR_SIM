using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.LightCones;

internal class ButtheBattleIsntOver : DefaultLightCone
{
    private readonly double[] modifiersDmg =
        { 0.3, 0.35, 0.4, 0.45, 0.5 };

    public ButtheBattleIsntOver(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path == Parent.Path)
        {
            //Sp regen at ulti
            foreach (var ability in Parent.Abilities.Where(x => x.AbilityType is Ability.AbilityTypeEnm.Ultimate))
            {
                PartyResourceGain spEnt = new(null, this, Parent.Parent)
                {
                    TargetUnit = Parent.Parent,
                    Val = 1,
                    ProcRatio = 2,
                    ResType = Resource.ResourceType.SP
                };

                ability.Events.Add(spEnt);
            }

            //AllDmgUp
            foreach (var ability in Parent.Abilities.Where(x => x.AbilityType is Ability.AbilityTypeEnm.Ability))
            {
                ApplyBuff allDmg = new(null, this, Parent.Parent)
                {
                    AppliedBuffToApply = new AppliedBuff(Parent.Parent)
                    {
                        Type = AppliedBuff.BuffType.Buff,
                        Effects = new List<Effect> { new EffAllDamageBoost { Value = modifiersDmg[Rank - 1] } },
                        BaseDuration = 1
                    }
                };

                ability.Events.Add(allDmg);
            }
        }
    }

    public override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Harmony;
}