using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.Fighters.LightCones;

internal abstract class DefaultLightCone : ILightCone
{
    public DefaultLightCone(IFighter parent, int rank)
    {
        Parent = parent;
        Rank = rank;
        if (Path != Parent.Path) //prevent from wrong Path lc
            return;
        EventHandlerProc += DefaultLightCone_HandleEvent;
        StepHandlerProc += DefaultLightCone_HandleStep;
    }


    public IFighter Parent { get; set; }
    public List<ConditionBuff> ConditionMods { get; set; } = new();
    public List<PassiveBuff> PassiveMods { get; set; } = new();
    public int Rank { get; set; }
    public abstract FighterUtils.PathType Path { get; }
    public ILightCone.EventHandler EventHandlerProc { get; set; }
    public ILightCone.StepHandler StepHandlerProc { get; set; }
    public List<Ability> Abilities { get; set; }

    public void Reset()
    {
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public virtual void DefaultLightCone_HandleEvent(Event ent)
    {
    }

    public virtual void DefaultLightCone_HandleStep(Step step)
    {
    }
}