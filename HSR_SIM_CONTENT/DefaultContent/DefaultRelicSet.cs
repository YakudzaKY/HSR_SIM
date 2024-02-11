using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Content;

public class DefaultRelicSet : IRelicSet
{
    public DefaultRelicSet(IFighter parent, int num)
    {
        Num = num;
        Parent = parent;
        EventHandlerProc += DefaultRelicSet_HandleEvent;
        StepHandlerProc += DefaultRelicSet_HandleStep;
    }

    public IFighter Parent { get; set; }

    public int Num { get; set; }
    public IRelicSet.EventHandler EventHandlerProc { get; set; }
    public IRelicSet.StepHandler StepHandlerProc { get; set; }
    public List<Ability> Abilities { get; set; }

    public void Reset()
    {
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public virtual void DefaultRelicSet_HandleEvent(Event ent)
    {
    }

    public virtual void DefaultRelicSet_HandleStep(Step step)
    {
    }
}