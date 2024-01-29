﻿using System;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// dispell buff or dot
public class RemoveBuff : BuffEventTemplate
{
    public RemoveBuff(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    private bool buffWasFound = false;
    public override string GetDescription()
    {
        return
            $"Remove modifications on {TargetUnit.Name}. Source: {Source?.GetType()?.ToString().Split(".").Last():s}";
    }

    public override void ProcEvent(bool revert)
    {
        //remove mod
        if (!revert)
            buffWasFound = TargetUnit.RemoveBuff(this, BuffToApply);
        else
        if (buffWasFound)
            TargetUnit.RestoreBuff(this, BuffToApply);



        base.ProcEvent(revert);
    }
}