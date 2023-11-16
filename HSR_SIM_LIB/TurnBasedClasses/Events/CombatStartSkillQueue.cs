using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class CombatStartSkillQueue : Event
{
    // insert technique skill to queue
    public CombatStartSkillQueue(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return "Queue ability to start skill queue";
    }

    public override void ProcEvent(bool revert)
    {
        //party buffs or opening
        if (!revert)
            ParentStep.Parent.BeforeStartQueue.Add(AbilityValue);
        else
            ParentStep.Parent.BeforeStartQueue.Remove(AbilityValue);
        base.ProcEvent(revert);
    }
}