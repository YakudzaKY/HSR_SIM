using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_LIB.Fighters.Relics.Set
{
    internal class InertSalsotto : DefaultRelicSet
    {
        public ConditionMod GetMod()
        {
            return new ConditionMod(Parent.Parent)
            {
                Mod = new Mod(Parent.Parent)
                {
                    Effects = new List<Effect>() { new Effect() { EffType = Effect.EffectType.AbilityTypeBoost, Value = 0.15, AbilityType =  Ability.AbilityTypeEnm.Ultimate  } ,
                                                    new Effect() { EffType = Effect.EffectType.AbilityTypeBoost, Value = 0.15, AbilityType =  Ability.AbilityTypeEnm.FollowUpAction  }},
                    CustomIconName = "gear\\" + GetType().ToString().Split('.').Last()
                },
                Target = Parent.Parent,
                Condition = new ConditionMod.ConditionRec()
                {
                    CondtionParam = ConditionMod.ConditionCheckParam.CritRate,
                    CondtionExpression = ConditionMod.ConditionCheckExpression.EqualOrMore,
                    Value = 0.50
                }
            };
        }
        public InertSalsotto(IFighter parent, int num) : base(parent, num)
        {
            if (num >= 2)
            {
                ConditionMods.Add(GetMod());
            }

        }
    }
}
