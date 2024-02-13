﻿using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// unit got defeated
public class Defeat : Event
{
    public Defeat(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {

    }

    public override string GetDescription()
    {
        return $"{TargetUnit.Name} got defeated";
    }

    public override void ProcEvent(bool revert)
    {

        if (!TriggersHandled)
        {

            //attacker got 10 energy
            ChildEvents.Add(new EnergyGain(ParentStep, TargetUnit, SourceUnit)
            { Val = 10, TargetUnit = SourceUnit });
            ChildEvents.Add(new SetLiveStatus(ParentStep, SourceUnit, SourceUnit)
            { ToState = Unit.LivingStatusEnm.Defeated, TargetUnit = TargetUnit });

        }

        base.ProcEvent(revert);
    }
}