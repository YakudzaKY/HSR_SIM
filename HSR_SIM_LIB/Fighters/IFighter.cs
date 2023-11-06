using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Fighters.FighterUtils;

namespace HSR_SIM_LIB.Fighters
{

    /// <summary>
    /// fighting logic interface
    /// </summary>
    public interface IFighter:ICloneable
    {
        public List<ConditionMod> ConditionMods { get; set; }
        public Unit GetBestTarget(Ability ability);
        public List<PassiveMod> PassiveMods { get; set; }
        //need of unique effect on shield break
        public Buff ShieldBreakMod { get; set; } 
        public Unit.ElementEnm Element { get;  }
        public PathType? Path { get; }
        public List<Unit.ElementEnm> Weaknesses { get; set; } 
        public List<Resist> Resists { get; set; }
        public List<DebuffResist> DebuffResists { get; set; } 
        public delegate void EventHandler(Event ent);
        public delegate void StepHandler(Step step);
        public EventHandler EventHandlerProc{ get; set; }
        public StepHandler StepHandlerProc{ get; set; }
        //ability list
        public List<Ability> Abilities { get; set; }
        public Unit Parent{ get; set; }
        public string GetSpecialText();//text for different triggers counters etc
        public double Cost { get;  }//unit cost in the squad
        public FighterUtils.UnitRole? Role { get;  }
        public void Reset();
        public  Ability ChoseAbilityToCast(Step step);

    }
}
