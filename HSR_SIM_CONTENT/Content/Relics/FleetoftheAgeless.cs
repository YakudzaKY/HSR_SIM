using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;

namespace HSR_SIM_CONTENT.Content.Relics;

internal class FleetoftheAgeless : DefaultRelicSet
{
    public FleetoftheAgeless(IFighter parent, int num) : base(parent, num)
    {
        if (num >= 2)
            Parent.Parent.PassiveBuffs.Add(new PassiveBuff(parent.Parent)
            {
                Effects = new List<Effect> { new EffAtkPrc { Value = 0.08 } },
                CustomIconName = "gear\\" + GetType().ToString().Split('.').Last(),

                Target = parent.Parent.ParentTeam,
                Condition = new PassiveBuff.ConditionRec
                {
                    ConditionParam = PassiveBuff.ConditionCheckParam.Spd,
                    ConditionExpression = PassiveBuff.ConditionCheckExpression.EqualOrMore,
                    Value = 120
                }
            });
    }
}