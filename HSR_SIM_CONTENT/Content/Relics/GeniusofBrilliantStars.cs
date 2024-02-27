using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.Relics;

internal class GeniusofBrilliantStars : DefaultRelicSet
{
    public GeniusofBrilliantStars(IFighter parent, int num) : base(parent, num)
    {
        if (num >= 2)
            Parent.Parent.PassiveBuffs.Add(new PassiveBuff(Parent.Parent)
            {
                Effects = new List<Effect> { new EffDefIgnore { Value = 0.10 } },
                Target = Parent.Parent
            });

        if (num >= 4)
            Parent.Parent.PassiveBuffs.Add(new PassiveBuff(parent.Parent)
            {
                Effects = new List<Effect> { new EffDefIgnore { Value = 0.10 } },
                IsTargetCheck = true,
                Target = Parent.Parent,
                Condition = new PassiveBuff.ConditionRec
                {
                    ConditionParam = PassiveBuff.ConditionCheckParam.Weakness,
                    ConditionExpression = PassiveBuff.ConditionCheckExpression.Exists,
                    ElemValue = Unit.ElementEnm.Quantum
                }
            });
    }
}