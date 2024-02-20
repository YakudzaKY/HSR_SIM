﻿using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// direct damage dealed
public class DirectDamage(Step parent, ICloneable source, Unit sourceUnit)
    : DamageEventTemplate(parent, source, sourceUnit)
{
    public bool IsCrit { get; set; }

    public override string GetDescription()
    {
        return "Dealing damage" + (IsCrit ? " (CRITICAL)" : "") +
               $" overall={Val:f} to_barrier={RealBarrierVal:f} to_hp={RealVal:f}";
    }

    public override void ProcEvent(bool revert)
    {
        DamageWorks(revert);
        base.ProcEvent(revert);
    }
}