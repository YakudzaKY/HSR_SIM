﻿using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// delete skill from queue(when techniqe skill executed in battle)
public class CombatStartSkillDeQueue : Event
{
    public CombatStartSkillDeQueue(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return "Remove ability from start skill queue";
    }

    public override void ProcEvent(bool revert)
    {
        //DEQUEUE party buffs or opening
        if (!revert)
            ParentStep.Parent.BeforeStartQueue.Remove(ParentStep.ActorAbility);
        else
            ParentStep.Parent.BeforeStartQueue.Add(ParentStep.ActorAbility);
        base.ProcEvent(revert);
    }
}