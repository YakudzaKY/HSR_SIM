﻿using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//modify Action Value
public class ModActionValue : Event
{
    public ModActionValue(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"Reduce {TargetUnit?.Name:s} action value on {Value:f}";
    }

    public override void ProcEvent(bool revert)
    {
        //if no target - reduce all units
        if (TargetUnit != null)
        {
            TargetUnit.Stats.PerformedActionValue += (double)(revert ? -Value : Value);
        }
        else
        {
            foreach (var unit in ParentStep.Parent.CurrentFight.AllAliveUnits)
                unit.Stats.PerformedActionValue += (double)(revert ? -Value : Value);
            if (!TriggersHandled)
                ParentStep.Parent.TotalAv += (double)Value;
        }

        base.ProcEvent(revert);
    }
}