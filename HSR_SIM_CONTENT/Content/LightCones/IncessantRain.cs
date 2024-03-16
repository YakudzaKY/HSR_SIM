using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CONTENT.Content.LightCones;

internal class IncessantRain : DefaultLightCone
{
    private readonly AppliedBuff? aetherCodeDebuff;

    private readonly double[] lcMods =
        [0.12, 0.14, 0.16, 0.18, 0.20];

    public IncessantRain(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path != Parent.Path) return;
        aetherCodeDebuff = new AppliedBuff(Parent.Parent,null,this)
        {
            Type = Buff.BuffType.Debuff, BaseDuration = 1,
            Effects = [new EffAllDamageVulnerability { Value = lcMods[Rank - 1] }]
        };


        Parent.Parent.PassiveBuffs.Add(new PassiveBuff(Parent.Parent, this)
        {
            Effects = [new EffCritPrc { CalculateValue = CalcCrit }],
            Target = Parent.Parent,
            IsTargetCheck = true
        });
    }


    public sealed override FighterUtils.PathType Path => FighterUtils.PathType.Nihility;

    //get 0.2 AllDmg per debuff  on target
    private double? CalcCrit(Event ent)
    {
        double debuffs = 0;
        debuffs += ent.TargetUnit?.AppliedBuffs.Count(x =>
            x.Type is Buff.BuffType.Debuff or Buff.BuffType.Dot) ?? 0;
        if (debuffs >= 3)
            return lcMods[Rank - 1];
        return 0;
    }

    protected override void DefaultLightCone_HandleEvent(Event ent)
    {
        if (ent is ExecuteAbilityFinish && aetherCodeDebuff != null && ent.SourceUnit == Parent.Parent &&
            ent.ParentStep.ActorAbility.Attack &&
            ent.ParentStep.ActorAbility.AbilityType != Ability.AbilityTypeEnm.Technique)
        {
            var targetHits =
                ent.ParentStep.TargetsHit.Where(x => x.AppliedBuffs.All(y => y.Reference != aetherCodeDebuff));
            if (targetHits.Any())
                ent.ChildEvents.Add(new AttemptEffect(ent.ParentStep, this, Parent.Parent)
                {
                    TargetUnit = (Unit)Utl.GetRandomObject(targetHits, Parent.Parent.ParentTeam.ParentSim.Parent),
                    BaseChance = 1,
                    AppliedBuffToApply = aetherCodeDebuff
                });
        }

        base.DefaultLightCone_HandleEvent(ent);
    }
}