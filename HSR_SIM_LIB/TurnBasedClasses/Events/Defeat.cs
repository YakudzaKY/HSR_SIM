﻿using System;
using System.Collections.Generic;
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
        return TargetUnit.Name + " get rekt (:";
    }

    public override void ProcEvent(bool revert)
    {
        //attacker got 10 energy
        if (!TriggersHandled)
            ChildEvents.Add(new EnergyGain(ParentStep, TargetUnit, AbilityValue.Parent.Parent)
                { Val = 10, TargetUnit = AbilityValue.Parent.Parent, AbilityValue = AbilityValue });
        //got defeated

        TargetUnit.IsAlive = revert;


        base.ProcEvent(revert);
    }
}