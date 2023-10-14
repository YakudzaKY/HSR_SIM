using System.Collections.Generic;
using static HSR_SIM_LIB.CallBacks;


namespace HSR_SIM_LIB.Fighters
{

    /// <summary>
    /// fighting logic interface
    /// </summary>
    public interface IFighter
    {
        public Unit.ElementEnm? Element { get; set; }
        public List<Unit.ElementEnm> Weaknesses { get; set; }
        public List<Resist> Resists { get; set; }
        public delegate void EventHandler(Event ent);
        public EventHandler EventHandlerProc{ get; set; }
        //ability list
        public List<Ability> Abilities { get; set; }
        public Unit Parent{ get; set; }
        public Ability ChooseAbilityToCast(Step step);

    }
}
