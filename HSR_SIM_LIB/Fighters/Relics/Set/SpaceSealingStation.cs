﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_LIB.Fighters.Relics.Set
{
    public class SpaceSealingStation:DefaultRelicSet
    {
        public ConditionMod GetMod()
        {
            return new ConditionMod(Parent.Parent)
            {
                Mod = new Mod(Parent.Parent)
                {
                    Effects = new List<Effect>() { new Effect() { EffType = Effect.EffectType.AtkPrc, Value = 0.12 } },
                    CustomIconName = "gear\\" + GetType().ToString().Split('.').Last()
                },
                Target = Parent.Parent,
                Condition = new ConditionMod.ConditionRec()
                {
                    CondtionParam = ConditionMod.ConditionCheckParam.SPD,
                    CondtionExpression = ConditionMod.ConditionCheckExpression.EqualOrMore,
                    Value = 120
                }
            };
        }

        public SpaceSealingStation(IFighter parent, int num) : base(parent, num)
        {
            if (num >= 2)
            {
                ConditionMods.Add(GetMod());
            }

        }
    }
}