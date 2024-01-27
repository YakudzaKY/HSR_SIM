using System;
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
    public List<Buff> RemovedMods { get; set; } = new();


    public override string GetDescription()
    {
        return TargetUnit.Name + " get rekt (:";
    }

    public override void ProcEvent(bool revert)
    {
        //attacker got 10 energy
        if (!TriggersHandled)
            ChildEvents.Add(new EnergyGain(ParentStep, TargetUnit, ParentStep.Actor)
                { Val = 10, TargetUnit = ParentStep.Actor });
        //got defeated
        //todo if have no EffRebirth
        TargetUnit.IsAlive = revert;
        TargetUnit.ParentTeam.ResetRoles();

        if (!revert)
            foreach (var mod in RemovedMods)
                TargetUnit.RemoveBuff(this, mod);
        else
            foreach (var mod in RemovedMods)
                TargetUnit.ApplyBuff(this, mod);
        //else todo: do rebirth event and remove buff also proceed rebirth follow up action

        base.ProcEvent(revert);
    }
}