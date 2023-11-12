using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;

namespace HSR_SIM_LIB.Fighters.Relics.Set;

internal class InertSalsotto : DefaultRelicSet
{
    public InertSalsotto(IFighter parent, int num) : base(parent, num)
    {
        if (num >= 2) ConditionMods.Add(GetMod());
    }

    public ConditionMod GetMod()
    {
        return new ConditionMod(Parent.Parent)
        {
            Mod = new Buff(Parent.Parent)
            {
                Effects = new List<Effect>
                {
                    new EffAbilityTypeBoost { Value = 0.15, AbilityType = Ability.AbilityTypeEnm.Ultimate },
                    new EffAbilityTypeBoost { Value = 0.15, AbilityType = Ability.AbilityTypeEnm.FollowUpAction }
                },
                CustomIconName = "gear\\" + GetType().ToString().Split('.').Last()
            },
            Target = Parent.Parent,
            Condition = new ConditionMod.ConditionRec
            {
                CondtionParam = ConditionMod.ConditionCheckParam.CritRate,
                CondtionExpression = ConditionMod.ConditionCheckExpression.EqualOrMore,
                Value = 0.50
            }
        };
    }
}