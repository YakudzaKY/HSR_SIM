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
        EventHandlerProcAfter += DefaultRelicSet_HandleEventAfter;
        StepHandlerProcAfter += DefaultRelicSet_HandleStepAfter;
        EventHandlerProcBefore+= DefaultRelicSet_HandleEventBefore;
        StepHandlerProcBefore += DefaultRelicSet_HandleStepBefore;
    }

    public IFighter Parent { get; set; }

    public int Num { get; set; }
    public IRelicSet.EventHandler EventHandlerProcAfter { get; set; }
    public IRelicSet.StepHandler StepHandlerProcAfter { get; set; }
    public IRelicSet.EventHandler EventHandlerProcBefore { get; set; }
    public IRelicSet.StepHandler StepHandlerProcBefore { get; set; }
    public List<Ability> Abilities { get; set; }

    public void Reset()
    {
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public virtual void DefaultRelicSet_HandleEventAfter(Event ent)
    {
    }

    public virtual void DefaultRelicSet_HandleStepAfter(Step step)
    {
    }

    public virtual void DefaultRelicSet_HandleEventBefore(Event ent)
    {
    }

    public virtual void DefaultRelicSet_HandleStepBefore(Step step)
    {
    }
}