using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_CONTENT.DefaultContent;

public abstract class DefaultLightCone : ILightCone
{
    protected DefaultLightCone(IFighter parent, int rank)
    {
        Parent = parent;
        Rank = rank;
        // ReSharper disable once VirtualMemberCallInConstructor
        if (Path != Parent.Path) //prevent from wrong Path lc
            return;
        EventHandlerProc += DefaultLightCone_HandleEvent;
        StepHandlerProc += DefaultLightCone_HandleStep;
    }


    protected IFighter Parent { get; set; }

    public int Rank { get; set; }
    public abstract FighterUtils.PathType Path { get; }
    public ILightCone.EventHandler? EventHandlerProc { get; set; }
    public ILightCone.StepHandler? StepHandlerProc { get; set; }

    public void Reset()
    {
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    protected virtual void DefaultLightCone_HandleEvent(Event ent)
    {
    }

    protected virtual void DefaultLightCone_HandleStep(Step step)
    {
    }
}