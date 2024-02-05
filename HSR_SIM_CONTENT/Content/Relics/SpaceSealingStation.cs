using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;

namespace HSR_SIM_CONTENT.Content.Relics;

public class SpaceSealingStation : DefaultRelicSet
{
    public SpaceSealingStation(IFighter parent, int num) : base(parent, num)
    {
        if (num >= 2) Parent.ConditionBuffs.Add(GetMod());
    }

    public ConditionBuff GetMod()
    {
        return new ConditionBuff(Parent.Parent)
        {
            AppliedBuff = new Buff(Parent.Parent)
            {
                Effects = new List<Effect> { new EffAtkPrc { Value = 0.12 } },
                CustomIconName = "gear\\" + GetType().ToString().Split('.').Last()
            },
            Target = Parent.Parent,
            Condition = new ConditionBuff.ConditionRec
            {
                CondtionParam = ConditionBuff.ConditionCheckParam.SPD,
                CondtionExpression = ConditionBuff.ConditionCheckExpression.EqualOrMore,
                Value = 120
            }
        };
    }
}