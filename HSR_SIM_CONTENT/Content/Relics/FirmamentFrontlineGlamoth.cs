using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;

namespace HSR_SIM_CONTENT.Content.Relics;

public class FirmamentFrontlineGlamoth: DefaultRelicSet
{
    public FirmamentFrontlineGlamoth(IFighter parent, int num) : base(parent, num)
    {
        if (num >= 2)
        {
            Parent.Parent.PassiveBuffs.Add(GetMod(135,0.12));
            Parent.Parent.PassiveBuffs.Add(GetMod(160,0.06));
        }
    }

    private PassiveBuff GetMod(double threshold,double bonus)
    {
        return new PassiveBuff(Parent.Parent, this)
        {
            Effects = new List<Effect> { new EffAllDamageBoost() { Value = bonus } },
            CustomIconName = GearIcon(),
            Target = Parent.Parent,
            ApplyConditions =
            [
                new Condition
                {
                    ConditionParam = Condition.ConditionCheckParam.Spd,
                    ConditionExpression = Condition.ConditionCheckExpression.EqualOrMore,
                    Value = threshold
                }
            ]
        };
    }
}