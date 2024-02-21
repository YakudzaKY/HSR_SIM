﻿using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// dispell buff or dot
public class RemoveBuff(Step parent, ICloneable source, Unit sourceUnit) : BuffEventTemplate(parent, source, sourceUnit)
{
    public override string GetDescription()
    {
        return
            $"Remove modifications on {TargetUnit.Name}. Source: {Source?.GetType()?.ToString().Split(".").Last():s}";
    }

    private Buff buffRemoved;

    public override void ProcEvent(bool revert)
    {
        //remove mod
        if (!revert)
        {
            buffRemoved = TargetUnit.RemoveBuff(this, BuffToApply);
            //err if buff not found on alive target
            if (buffRemoved == null && TargetUnit.LivingStatus != Unit.LivingStatusEnm.Defeated)
                throw new Exception("Cant find buff to remove:(");
        }
        else if (buffRemoved != null)
            TargetUnit.RestoreBuff(this, buffRemoved);

        base.ProcEvent(revert);
    }
}