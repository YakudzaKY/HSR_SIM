using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// command to start battle(when combat technique used)
public class EnterCombat : Event
{
    public EnterCombat(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return "entering the combat...";
    }

    public override void ProcEvent(bool revert)
    {
        //entering combat
        ParentStep.Parent.DoEnterCombat = !revert;
        base.ProcEvent(revert);
    }
}