﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Relics.Set
{
    internal class GeniusofBrilliantStars : DefaultRelicSet
    {
        public GeniusofBrilliantStars(IFighter parent, int num) : base(parent, num)
        {
            if (num >= 2)
                PassiveMods.Add(new PassiveMod(Parent.Parent)
                {
                    Mod = new Mod(Parent.Parent)
                    { Effects = new List<Effect>() { new Effect() { EffType = Effect.EffectType.DefIgnore, Value = 0.10 } } },
                    Target = Parent.Parent

                });

            if (num >= 4)
                ConditionMods.Add(new ConditionMod(parent.Parent)
                {
                    Mod = new Mod(Parent.Parent) { Effects = new List<Effect>() { new Effect() { EffType = Effect.EffectType.DefIgnore, Value = 0.10 } } }
                    ,
                    IsTargetCheck = true
                    ,
                    Target = Parent.Parent
                    ,
                    Condition = new ConditionMod.ConditionRec() { CondtionParam = ConditionMod.ConditionCheckParam.Weakness, CondtionExpression = ConditionMod.ConditionCheckExpression.EqualOrMore, ElemValue = Unit.ElementEnm.Quantum }

                });

        }
    }
}
