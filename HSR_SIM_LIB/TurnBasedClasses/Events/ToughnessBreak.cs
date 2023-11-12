using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//Break the shield
public class ToughnessBreak : DamageEventTemplate
{
    public ToughnessBreak(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return TargetUnit.Name + " shield broken " +
               $" overall={Val:f} to_hp={RealVal:f}";
    }

    public override void ProcEvent(bool revert)
    {
        DamageWorks(revert);
        base.ProcEvent(revert);
    }
}