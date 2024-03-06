using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;

namespace HSR_SIM_CONTENT.Content.Relics;

public class SpaceSealingStation : DefaultRelicSet
{
    public SpaceSealingStation(IFighter parent, int num) : base(parent, num)
    {
        if (num >= 2) Parent.Parent.PassiveBuffs.Add(GetMod());
    }

    private PassiveBuff GetMod()
    {
        return new PassiveBuff(Parent.Parent,this)
        {
            Effects = new List<Effect> { new EffAtkPrc { Value = 0.12 } },
            CustomIconName = "gear\\" + GetType().ToString().Split('.').Last(),
            Target = Parent.Parent,
            Condition = new PassiveBuff.ConditionRec
            {
                ConditionParam = PassiveBuff.ConditionCheckParam.Spd,
                ConditionExpression = PassiveBuff.ConditionCheckExpression.EqualOrMore,
                Value = 120
               
            }
        };
    }
}