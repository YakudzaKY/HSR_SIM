using System;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Content;

/// <summary>
///     Relic set  interface. Same as IFighter but simple
/// </summary>
public interface IRelicSet : ICloneable
{
    public delegate void EventHandler(Event ent);

    public delegate void StepHandler(Step step);

    public int Num { get; set; }

    public EventHandler EventHandlerProc { get; set; }
    public StepHandler StepHandlerProc { get; set; }
    public void Reset();
}