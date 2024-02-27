using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

internal class UnbindUnit : Event

{
    private Team lastTeam;
    private int ndx;

    public UnbindUnit(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
    {
    }

    public override void ProcEvent(bool revert)
    {
        if (!revert)
        {
            lastTeam = TargetUnit.ParentTeam;
            ndx = lastTeam.UnBindUnit(TargetUnit);
        }
        else
        {
            lastTeam.BindUnit(TargetUnit, ndx);
        }

        base.ProcEvent(revert);
    }

    public override string GetDescription()
    {
        return $"unbind {TargetUnit.Name} from team";
    }
}