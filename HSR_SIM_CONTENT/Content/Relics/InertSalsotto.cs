using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;

namespace HSR_SIM_CONTENT.Content.Relics;

internal class InertSalsotto : DefaultRelicSet
{
    public InertSalsotto(IFighter parent, int num) : base(parent, num)
    {
        if (num >= 2) Parent.Parent.PassiveBuffs.Add(GetMod());
    }

    private PassiveBuff GetMod()
    {
        return new PassiveBuff(Parent.Parent, this)
        {
            Effects =
            [
                new EffAbilityTypeBoost { Value = 0.15, AbilityType = Ability.AbilityTypeEnm.Ultimate },
                new EffAbilityTypeBoost { Value = 0.15, AbilityType = Ability.AbilityTypeEnm.FollowUpAction }
            ],
            CustomIconName = "gear\\" + GetType().ToString().Split('.').Last(),

            Target = Parent.Parent,
            ApplyConditions = [new Condition
            {
                ConditionParam = Condition.ConditionCheckParam.CritRate,
                ConditionExpression = Condition.ConditionCheckExpression.EqualOrMore,
                Value = 0.50
            }]
        };
    }
}