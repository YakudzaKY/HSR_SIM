using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_CONTENT.Content.Relics;

/// <summary>
///     new set with  debuff dmg increase PioneerDiverofDeadWaters
/// </summary>
internal class PioneerDiverofDeadWaters : DefaultRelicSet
{
    private readonly AppliedBuff onDebuffHitAppliedBuff;

    public PioneerDiverofDeadWaters(IFighter parent, int num) : base(parent, num)
    {
        onDebuffHitAppliedBuff = new AppliedBuff(Parent.Parent,null,this)
            { BaseDuration = 1, Dispellable = false, CustomIconName = "Sword", Effects = [] };
        if (num >= 2)
            Parent.Parent.PassiveBuffs.Add(new PassiveBuff(parent.Parent, this)
            {
                Effects = new List<Effect> { new EffAllDamageBoost { Value = 0.12 } },
                IsTargetCheck = true,
                Target = Parent.Parent,
                ApplyConditions = [new Condition
                {
                    ConditionParam = Condition.ConditionCheckParam.AnyDebuff,
                    ConditionExpression = Condition.ConditionCheckExpression.Exists
                }]
            });

        if (num >= 4)
            Parent.Parent.PassiveBuffs.Add(new PassiveBuff(parent.Parent, this)
            {
                Effects = new List<Effect> { new EffCritDmg { CalculateValue = Calc4Pieces } },
                IsTargetCheck = true,
                Target = Parent.Parent,
                ApplyConditions = [new Condition
                {
                    ConditionParam = Condition.ConditionCheckParam.AnyDebuff,
                    ConditionExpression = Condition.ConditionCheckExpression.Exists
                }]
            });
    }

    public override void DefaultRelicSet_HandleEvent(Event ent)
    {
        if (Num >= 4)
            if (ent is ApplyBuff ab
                && ent.SourceUnit == Parent.Parent
                && ent.TargetUnit.ParentTeam != Parent.Parent.ParentTeam
                && ab.AppliedBuffToApply.Type != Buff.BuffType.Buff
               )
            {
                //only one proc per action 

                ApplyBuff newEvent = new(ent.ParentStep, this, Parent.Parent)
                {
                    TargetUnit = Parent.Parent,
                    AppliedBuffToApply = onDebuffHitAppliedBuff
                };
                ent.ChildEvents.Add(newEvent);
            }


        base.DefaultRelicSet_HandleEvent(ent);
    }

    private double? Calc4Pieces(Event ent)
    {
        var debuffs = ent.TargetUnit.AppliedBuffs.Count(x =>
            x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);

        var mod = ent.SourceUnit.AppliedBuffs.Any(x => x.Reference == onDebuffHitAppliedBuff) ? 2 : 1;
        return debuffs switch
        {
            >= 3 => 0.12 * mod,
            2 => 0.08 * mod,
            _ => 0
        };
    }
}