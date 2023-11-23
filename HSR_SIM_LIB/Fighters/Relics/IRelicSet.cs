using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Fighters.Relics;

public interface IRelicSet : ICloneable
{
    public delegate void EventHandler(Event ent);

    public delegate void StepHandler(Step step);

    public List<PassiveBuff> PassiveMods { get; set; }
    public List<ConditionBuff> ConditionMods { get; set; }
    public int Num { get; set; }

    public EventHandler EventHandlerProc { get; set; }
    public StepHandler StepHandlerProc { get; set; }
    public List<Ability> Abilities { get; set; }
    public void Reset();
}