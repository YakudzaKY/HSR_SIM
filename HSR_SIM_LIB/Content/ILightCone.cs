using System;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using static HSR_SIM_LIB.Content.FighterUtils;

namespace HSR_SIM_LIB.Content;

/// <summary>
///     Light cone interface. Same as IFighter but simple
/// </summary>
public interface ILightCone : ICloneable
{
    public delegate void EventHandler(Event ent);

    public delegate void StepHandler(Step step);


    public int Rank { get; set; }
    public PathType Path { get; }

    public EventHandler EventHandlerProc { get; set; }
    public StepHandler StepHandlerProc { get; set; }

    public void Reset();
}