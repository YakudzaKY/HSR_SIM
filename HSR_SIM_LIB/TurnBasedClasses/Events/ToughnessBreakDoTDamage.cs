using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class ToughnessBreakDoTDamage(Step parent, ICloneable source, Unit sourceUnit, Unit.ElementEnm element)
    : DoTDamage(parent, source, sourceUnit, element)
{
    public override string GetDescription()
    {
        return $"DoT(Shield Break) tick from {SourceUnit.Name}" +
               $" overall={Value:f} to_barrier={RealBarrierVal:f} to_hp={RealValue:f}";
    }
}