using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_CONTENT.Content.LightCones;

internal class AcheronLcTst : DefaultLightCone
{
    private readonly AppliedBuff? aetherCodeDebuff;

    private readonly double[] lcMods =
        [0.12, 0.14, 0.16, 0.18, 0.40];

    public AcheronLcTst(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path != Parent.Path) return;
        aetherCodeDebuff = new AppliedBuff(Parent.Parent,null,this)
        {
            CustomIconName = "defeat",
            Type = Buff.BuffType.Debuff,
            BaseDuration = 1,
            Effects = []
        };


        Parent.Parent.PassiveBuffs.Add(new PassiveBuff(Parent.Parent, this)
        {
            Effects =
            [
                new EffAllDamageBoost { Value = lcMods[Rank - 1] },
                new EffAbilityTypeBoost
                    { Value = lcMods[Rank - 1], AbilityType = Ability.AbilityTypeEnm.Ultimate }
            ],

            Target = Parent.Parent,
            ApplyConditions = [new Condition
            {
                AppliedBuffValue = aetherCodeDebuff,
                ConditionExpression = Condition.ConditionCheckExpression.Exists,
                ConditionParam = Condition.ConditionCheckParam.Buff
            }],
            IsTargetCheck = true
        });
    }


    public sealed override FighterUtils.PathType Path => FighterUtils.PathType.Nihility;

    public override void DefaultLightCone_HandleEvent(Event ent)
    {
        if (ent is ExecuteAbilityFinish && aetherCodeDebuff != null && ent.SourceUnit == Parent.Parent &&
            ent.ParentStep.ActorAbility.Attack &&
            ent.ParentStep.ActorAbility.AbilityType != Ability.AbilityTypeEnm.Technique)
        {
            var targetHits =
                ent.ParentStep.TargetsHit.Where(x => x.AppliedBuffs.All(y => y.Reference != aetherCodeDebuff));
            foreach (var unit in targetHits)
                ent.ChildEvents.Add(new AttemptEffect(ent.ParentStep, this, Parent.Parent)
                {
                    TargetUnit = unit,
                    BaseChance = 1,
                    AppliedBuffToApply = aetherCodeDebuff
                });
        }

        base.DefaultLightCone_HandleEvent(ent);
    }
}