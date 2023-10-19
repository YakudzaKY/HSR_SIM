﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Relics
{
    public class DefaultRelicSet:IRelicSet
    {
        public List<PassiveMod> PassiveMods { get; set; }=new List<PassiveMod>();
        public List<ConditionMod> ConditionMods { get; set; }=new List<ConditionMod>();
        public int num { get; set; }
        public IRelicSet.EventHandler EventHandlerProc { get; set; }
        public IRelicSet.StepHandler StepHandlerProc { get; set; }
        public List<Ability> Abilities { get; set; }

        public DefaultRelicSet(IFighter parent,int num)
        {
            this.num = num;
            Parent = parent;
            EventHandlerProc += DefaultRelicSet_HandleEvent;
            StepHandlerProc += DefaultRelicSet_HandleStep;

        }

        public IFighter Parent { get; set; }

        public virtual void DefaultRelicSet_HandleEvent(Event ent)
        {
            
           
        }
        public virtual  void DefaultRelicSet_HandleStep(Step step)
        {
            
            
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
