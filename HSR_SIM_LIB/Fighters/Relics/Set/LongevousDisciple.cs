﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Relics.Set
{
    internal class LongevousDisciple : DefaultRelicSet
    {
        private readonly Buff uniqueBuff = null;

        public LongevousDisciple(IFighter parent,int num) : base(parent,num)
        {
            uniqueBuff = new Buff(Parent.Parent)
            {
                Type = Buff.ModType.Buff, BaseDuration = 2, MaxStack = 2, 
                Effects= new (){new EffCritPrc(){ Value = 0.08}}
            };
        }

        public override  void DefaultRelicSet_HandleEvent(Event ent)
        {
            //if friend unit consume our hp or got attack then apply buff
            if (Num >= 4)
            {
                if (( ent is ResourceDrain
                      &&ent.AbilityValue?.Parent.Parent.ParentTeam==Parent.Parent.ParentTeam
                      && ent.TargetUnit == Parent.Parent
                      && ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal != 0)
                    || (ent.TargetUnit == Parent.Parent && ent is DirectDamage))
                {
                    ApplyBuff newEvent = new (ent.Parent, this, Parent.Parent)
                    {
                        TargetUnit = Parent.Parent,
                        BuffToApply = uniqueBuff
                    };
                    ent.ChildEvents.Add(newEvent);
                }
            }


            base.DefaultRelicSet_HandleEvent(ent);
           
        }
        public override  void DefaultRelicSet_HandleStep(Step step)
        {
            
            base.DefaultRelicSet_HandleStep(step);
        }
    }
}
