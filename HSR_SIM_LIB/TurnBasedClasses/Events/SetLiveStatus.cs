using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class SetLiveStatus(Step parentStep, ICloneable source, Unit sourceUnit)
    : Event(parentStep, source, sourceUnit)
{
    private Unit.LivingStatusEnm fromState;
    public Unit.LivingStatusEnm ToState { get; set; }
    public List<AppliedBuff> RemovedMods { get; set; } = new();

    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
        {
            fromState = TargetUnit.LivingStatus;
            if (ToState == Unit.LivingStatusEnm.WaitingForFollowUp)
                RemovedMods.AddRange(TargetUnit.AppliedBuffs.Where(x => x.Dispellable));
            if (ToState == Unit.LivingStatusEnm.Defeated)
                RemovedMods.AddRange(TargetUnit.AppliedBuffs);
        }

        if (!revert)
        {
            foreach (var mod in RemovedMods)
                TargetUnit.RemoveBuff(this, mod);
            TargetUnit.LivingStatus = ToState;
        }
        else
        {
            List<AppliedBuff> rmods = new();
            rmods.AddRange(RemovedMods);
            rmods.Reverse();
            foreach (var mod in rmods)
                TargetUnit.RestoreBuff(this, mod);

            TargetUnit.LivingStatus = fromState;
        }

        TargetUnit.ParentTeam.ResetRoles();
        //need reset enemy team roles(if 1 enemy is alive then Hunt>Destruction)
        TargetUnit.EnemyTeam.ResetRoles();

        base.ProcEvent(revert);
    }

    public override string GetDescription()
    {
        return $"set {TargetUnit.Name} state to {ToState.ToString()}";
    }
}