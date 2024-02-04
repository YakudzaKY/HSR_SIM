using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Relics.Set
{
    internal class test : DefaultRelicSet
    {
        private Buff onDebuffHitBuff;
        public test(IFighter parent, int num) : base(parent, num)
        {
            onDebuffHitBuff = new Buff(Parent.Parent, null) { BaseDuration = 1,Dispellable = false,CustomIconName = "Sword"};
            if (num >= 2)
                Parent.ConditionBuffs.Add(new ConditionBuff(parent.Parent)
                {
                    AppliedBuff = new Buff(Parent.Parent) { Effects = new List<Effect> { new EffAllDamageBoost() { Value = 0.12 } } },
                    IsTargetCheck = true,
                    Target = Parent.Parent,
                    Condition = new ConditionBuff.ConditionRec
                    {
                        CondtionParam = ConditionBuff.ConditionCheckParam.AnyDebuff,
                        CondtionExpression = ConditionBuff.ConditionCheckExpression.Exists,
                    }
                });

            if (num >= 4)
                Parent.ConditionBuffs.Add(new ConditionBuff(parent.Parent)
                {
                    AppliedBuff = new Buff(Parent.Parent) { Effects = new List<Effect> { new EffCritDmg() { CalculateValue = Calc4Pieces , RealTimeRecalculateValue=true} } },
                    IsTargetCheck = true,
                    Target = Parent.Parent,
                    Condition = new ConditionBuff.ConditionRec
                    {
                        CondtionParam = ConditionBuff.ConditionCheckParam.AnyDebuff,
                        CondtionExpression = ConditionBuff.ConditionCheckExpression.Exists,
                    }
                });
        }

        public override void DefaultRelicSet_HandleEvent(Event ent)
        {
            if (Num >= 4)
                if (ent is ApplyBuff ab
                     && ent.SourceUnit==Parent.Parent
                     && ent.TargetUnit.ParentTeam != Parent.Parent.ParentTeam
                     && ab.BuffToApply.Type != Buff.BuffType.Buff
                    )
                {
                    //only one proc per action 
   
                    ApplyBuff newEvent = new(ent.ParentStep, this, Parent.Parent)
                    {
                        TargetUnit = Parent.Parent,
                        BuffToApply = onDebuffHitBuff
                    };
                    ent.ChildEvents.Add(newEvent);
                }


            base.DefaultRelicSet_HandleEvent(ent);
        }

        private  double? Calc4Pieces(Event ent)
        {
            double maxDebuffs = 5;
            double debuffs = 0;
            double mod;

            debuffs = ent.TargetUnit.Buffs.Count(x => x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);

            if (ent.SourceUnit.Buffs.Any(x => x.Reference == onDebuffHitBuff))
                mod = 2;
            else
                mod = 1;
            if (debuffs >= 3)
                return 0.12 * mod;
            if (debuffs == 2)
                return 0.08 * mod;
            return 0;

        }
    }
}
