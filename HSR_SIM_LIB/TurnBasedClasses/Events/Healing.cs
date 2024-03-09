using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class Healing : Event
{
    public Healing(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"Healing {TargetUnit.Name} " +
               $" overall={Value:f} to_hp={RealValue:f} Source: {Source.GetType().Name}";
    }

    public override void ProcEvent(bool revert)
    {
        var res = TargetUnit.GetRes(Resource.ResourceType.HP);
        if (RealValue == null)
            //cant overheal
            RealValue = Math.Min(TargetUnit.GetMaxHp(null) - res.ResVal, (double)Value);


        res.ResVal += (double)(revert ? -RealValue : RealValue);
        base.ProcEvent(revert);
    }
}