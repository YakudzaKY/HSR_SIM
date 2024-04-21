using System;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

/// <summary>
///     Energy gain. Affected by Energy regen rate.
///     For raw energy gain use ResourceGain instead
/// </summary>
public class EnergyGain(Step parent, ICloneable source, Unit sourceUnit) : Event(parent, source, sourceUnit)
{
    //scale energy gain by energy regen rate
    private bool IsRawEnergy { get; } = false;


    public override string GetDescription()
    {
        return $"{TargetUnit.Name} Gain energy:{Value:f} final value: {RealValue:f} source: {Source.GetType().Name}";
    }


    public override void ProcEvent(bool revert)
    {
        //switch value to formula
        if (Value != null && CalculateValue == null)
        {
            CalculateValue = new Formula()
            {
                EventRef = this, Expression = $"{Value}"+
                                              //if RawEnergy then value does not affected by regen rate
                                              (IsRawEnergy?String.Empty : $" *  {Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.EnergyRegenPrc)} ")
            };
            Value = null;
        }

        if (RealValue == null)
        {
            RealValue = Value;//will trigger formula calculation
            RealValue = Math.Min(Value??0, TargetUnit.Fighter.MaxEnergy - TargetUnit.CurrentEnergy);
        }

        TargetUnit.CurrentEnergy += (double)(revert ? -RealValue : RealValue);
        base.ProcEvent(revert);
    }
}