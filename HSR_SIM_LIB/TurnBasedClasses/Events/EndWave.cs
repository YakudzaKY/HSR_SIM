﻿using System;
using System.Collections.Generic;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// End the wave
public class EndWave : Event
{
    public EndWave(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public List<Unit> UnloadedUnits { get; set; } = new();

    public override string GetDescription()
    {
        return "end wave";
    }

    public override void ProcEvent(bool revert)
    {
        //Unload

        if (!revert)
        {
            if (!TriggersHandled)
                UnloadedUnits.AddRange(ParentStep.Parent.HostileTeam.Units);

            ParentStep.Parent.HostileTeam.UnBindUnits();
            ParentStep.Parent.CurrentFight.CurrentWave = null;
        }
        else
        {
            ParentStep.Parent.CurrentFight.CurrentWave =
                ParentStep.Parent.CurrentFight.ReferenceFight.Waves[ParentStep.Parent.CurrentFight.CurrentWaveCnt - 1];
            ParentStep.Parent.HostileTeam.BindUnits(UnloadedUnits);
        }

        base.ProcEvent(revert);
    }
}