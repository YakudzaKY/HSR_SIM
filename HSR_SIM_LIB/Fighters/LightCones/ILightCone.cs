﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;

namespace HSR_SIM_LIB.Fighters.LightCones
{
    /// <summary>
    /// Light cone/relics interface
    /// </summary>
    public interface ILightCone
    {
        public delegate void EventHandler(Event ent);
        public delegate void StepHandler(Step step);
        public int Rank { get; set; }
        

        public EventHandler EventHandlerProc{ get; set; }
        public StepHandler StepHandlerProc{ get; set; }
        public List<Ability> Abilities { get; set; }
        
    }
}
