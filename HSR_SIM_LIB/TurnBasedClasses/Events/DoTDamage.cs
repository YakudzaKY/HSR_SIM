using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

// Damage by DoTs(when turn started)
public class DoTDamage : DamageEventTemplate
{
    public Unit.ElementEnm Element { get; set; }
    public DoTDamage(Step parent, ICloneable source, Unit sourceUnit,Unit.ElementEnm element) : base(parent, source, sourceUnit)
    {
        Element = element;
    }

    public override string GetDescription()
    {
        return $"DoT tick from {SourceUnit.Name}" +
               $" overall={Val:f} to_barier={RealBarrierVal:f} to_hp={RealVal:f}";
    }

    public override void ProcEvent(bool revert)
    {
        DamageWorks(revert);
        base.ProcEvent(revert);
    }
}