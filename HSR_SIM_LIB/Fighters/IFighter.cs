using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters
{

    /// <summary>
    /// fighting logic interface
    /// </summary>
    public interface IFighter:ICloneable
    {
        public List<ConditionMod> ConditionMods { get; set; }
        public List<PassiveMod> PassiveMods { get; set; }
        public FighterUtils.PathType? Path { get; set; }
        public Unit.ElementEnm Element { get; set; }
        public List<Unit.ElementEnm> Weaknesses { get; set; }
        public List<Resist> Resists { get; set; }
        public delegate void EventHandler(Event ent);
        public delegate void StepHandler(Step step);
        public EventHandler EventHandlerProc{ get; set; }
        public StepHandler StepHandlerProc{ get; set; }
        //ability list
        public List<Ability> Abilities { get; set; }
        public Unit Parent{ get; set; }
        public Ability ChooseAbilityToCast(Step step);
        public string GetSpecialText();//text for different triggers counters etc
        

    }
}
