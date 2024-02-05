using System.Collections.Generic;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.Relics;

internal class GeniusofBrilliantStars : DefaultRelicSet
{
    public GeniusofBrilliantStars(IFighter parent, int num) : base(parent, num)
    {
        if (num >= 2)
            Parent.PassiveBuffs.Add(new PassiveBuff(Parent.Parent)
            {
                AppliedBuff = new Buff(Parent.Parent)
                { Effects = new List<Effect> { new EffDefIgnore { Value = 0.10 } } },
                Target = Parent.Parent
            });

        if (num >= 4)
            Parent.ConditionBuffs.Add(new ConditionBuff(parent.Parent)
            {
                AppliedBuff = new Buff(Parent.Parent) { Effects = new List<Effect> { new EffDefIgnore { Value = 0.10 } } },
                IsTargetCheck = true,
                Target = Parent.Parent,
                Condition = new ConditionBuff.ConditionRec
                {
                    CondtionParam = ConditionBuff.ConditionCheckParam.Weakness,
                    CondtionExpression = ConditionBuff.ConditionCheckExpression.Exists,
                    ElemValue = Unit.ElementEnm.Quantum
                }
            });
    }
}