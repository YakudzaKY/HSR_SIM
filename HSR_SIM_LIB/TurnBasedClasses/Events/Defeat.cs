using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// unit defeated( usefull for geppard etc)
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
            ChildEvents.Add(new EnergyGain(Parent, null, AbilityValue.Parent.Parent)
                { Val = 10, TargetUnit = AbilityValue.Parent.Parent, AbilityValue = AbilityValue });
        //got defeated

        TargetUnit.IsAlive = revert;

        if (!revert)
            foreach (var mod in RemovedMods)
                TargetUnit.RemoveBuff(this, mod);
        else
            foreach (var mod in RemovedMods)
                TargetUnit.ApplyBuff(this, mod);

        base.ProcEvent(revert);
    }
}