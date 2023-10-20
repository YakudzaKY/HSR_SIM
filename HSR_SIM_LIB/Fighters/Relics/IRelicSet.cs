﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;

namespace HSR_SIM_LIB.Fighters.Relics
{
    public interface IRelicSet:ICloneable
    {
        public delegate void EventHandler(Event ent);
        public delegate void StepHandler(Step step);
        public List<PassiveMod> PassiveMods { get; set; }
        public List<ConditionMod> ConditionMods { get; set; }
        public int num { get; set; }

        public EventHandler EventHandlerProc{ get; set; }
        public StepHandler StepHandlerProc{ get; set; }
        public List<Ability> Abilities { get; set; }
    }
}