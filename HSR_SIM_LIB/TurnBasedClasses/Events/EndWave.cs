using System;
using System.Collections.Generic;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// End the wave
public class EndWave : Event
{
    public EndWave(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public List<Unit> UnloadedUnits { get; set; }

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
                UnloadedUnits = Parent.Parent.HostileTeam.Units;

            Parent.Parent.HostileTeam.UnBindUnits();
            Parent.Parent.CurrentFight.CurrentWave = null;
        }
        else
        {
            Parent.Parent.CurrentFight.CurrentWave =
                Parent.Parent.CurrentFight.ReferenceFight.Waves[Parent.Parent.CurrentFight.CurrentWaveCnt - 1];
            Parent.Parent.HostileTeam.BindUnits(UnloadedUnits);
        }

        base.ProcEvent(revert);
    }
}