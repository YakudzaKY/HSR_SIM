using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Content;

public abstract class DefaultLightCone : ILightCone
{
    public DefaultLightCone(IFighter parent, int rank)
    {
        Parent = parent;
        Rank = rank;
        if (Path != Parent.Path) //prevent from wrong Path lc
            return;
        EventHandlerProcAfter += DefaultLightCone_HandleEventAfter;
        StepHandlerProcAfter += DefaultLightCone_HandleStepAfter;
        EventHandlerProcBefore += DefaultLightCone_HandleEventBefore;
        StepHandlerProcBefore += DefaultLightCone_HandleStepBefore;
    }


    public IFighter Parent { get; set; }
    public List<ConditionBuff> ConditionMods { get; set; } = new();
    public List<PassiveBuff> PassiveMods { get; set; } = new();
    public int Rank { get; set; }
    public abstract FighterUtils.PathType Path { get; }
    public ILightCone.EventHandler EventHandlerProcAfter { get; set; }
    public ILightCone.StepHandler StepHandlerProcAfter { get; set; }
    public ILightCone.EventHandler EventHandlerProcBefore { get; set; }
    public ILightCone.StepHandler StepHandlerProcBefore { get; set; }
    public List<Ability> Abilities { get; set; }

    public void Reset()
    {
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public virtual void DefaultLightCone_HandleEventAfter(Event ent)
    {
    }

    public virtual void DefaultLightCone_HandleStepAfter(Step step)
    {
    }

    public virtual void DefaultLightCone_HandleEventBefore(Event ent)
    {
    }

    public virtual void DefaultLightCone_HandleStepBefore(Step step)
    {
    }
}