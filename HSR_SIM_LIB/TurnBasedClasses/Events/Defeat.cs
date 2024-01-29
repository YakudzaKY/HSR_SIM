using System;
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
    public List<Buff> RemovedMods { get; set; } = new();



    public override string GetDescription()
    {
        return $"{TargetUnit.Name} got defeated";
    }

    public override void ProcEvent(bool revert)
    {

        if (!TriggersHandled)
        {

            //attacker got 10 energy
            RemovedMods.AddRange(TargetUnit.Buffs);
            ChildEvents.Add(new EnergyGain(ParentStep, TargetUnit, SourceUnit)
            { Val = 10, TargetUnit = SourceUnit });
            ChildEvents.Add(new SetLiveStatus(ParentStep, SourceUnit, SourceUnit)
            { ToState = Unit.LivingStatusEnm.Defeated, TargetUnit = TargetUnit });

        }


        if (!revert)
        {

            foreach (var mod in RemovedMods)
                TargetUnit.RemoveBuff(this, mod);
        }
        else
        {
            List<Buff> rmods = new();
            rmods.AddRange(RemovedMods);
            rmods.Reverse();
            foreach (var mod in rmods)
                TargetUnit.RestoreBuff(this, mod);
        }


        base.ProcEvent(revert);
    }
}