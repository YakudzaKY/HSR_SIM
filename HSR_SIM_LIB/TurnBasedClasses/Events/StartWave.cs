using System;
using System.Collections.Generic;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// starting the wave
public class StartWave : Event
{
    public StartWave(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public List<Unit> StartingUnits { get; set; }

    public override string GetDescription()
    {
        return "next wave";
    }

    public override void ProcEvent(bool revert)
    {
        //Loading wave

        if (!revert)
        {
            ParentStep.Parent.CurrentFight.CurrentWaveCnt += 1;
            ParentStep.Parent.CurrentFight.CurrentWave =
                ParentStep.Parent.CurrentFight.ReferenceFight.Waves[ParentStep.Parent.CurrentFight.CurrentWaveCnt - 1];
            StartingUnits ??= SimCls.GetCombatUnits(ParentStep.Parent.CurrentFight.CurrentWave.Units);
            ParentStep.Parent.HostileTeam.BindUnits(StartingUnits);
            //set start action value
            foreach (var unit in ParentStep.Parent.AllUnits) unit.Stats.ResetAV();
        }
        else
        {
            ParentStep.Parent.HostileTeam.UnBindUnits();
            ParentStep.Parent.CurrentFight.CurrentWaveCnt -= 1;
            ParentStep.Parent.CurrentFight.CurrentWave = null;
        }

        base.ProcEvent(revert);
    }
}