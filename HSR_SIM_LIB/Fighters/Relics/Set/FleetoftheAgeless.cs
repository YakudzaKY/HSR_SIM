using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;

namespace HSR_SIM_LIB.Fighters.Relics.Set;

internal class FleetoftheAgeless : DefaultRelicSet
{
    public FleetoftheAgeless(IFighter parent, int num) : base(parent, num)
    {
        if (num >= 2)
            ConditionMods.Add(new ConditionBuff(parent.Parent)
            {
                Mod = new Buff(Parent.Parent)
                {
                    Effects = new List<Effect> { new EffAtkPrc { Value = 0.08 } },
                    CustomIconName = "gear\\" + GetType().ToString().Split('.').Last()
                },
                Target = parent.Parent.ParentTeam,
                Condition = new ConditionBuff.ConditionRec
                {
                    CondtionParam = ConditionBuff.ConditionCheckParam.SPD,
                    CondtionExpression = ConditionBuff.ConditionCheckExpression.EqualOrMore, Value = 120
                }
            });
    }
}