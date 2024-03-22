using System;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//drain some resource
public class ResourceDrain : Event
{
    public ResourceDrain(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public Resource.ResourceType ResType { get; set; }

    public bool CanSetToZero { get; init; } = true;

    public override string GetDescription()
    {
        return TargetUnit.Name + " res drain : " + Value + " " + ResType + "(by " + SourceUnit.Name + ")";
    }

    public override void ProcEvent(bool revert)
    {
        //Resource drain

        if (RealValue == null)
        {
            RealValue = Math.Min(TargetUnit.GetRes(ResType).ResVal, (double)Value);
            if (!CanSetToZero && RealValue >= TargetUnit.GetRes(ResType).ResVal) RealValue -= 1;
            //Game mechanics depend on Val instead
            Value = RealValue;
        }

        TargetUnit.GetRes(ResType).ResVal += (double)-(revert ? -RealValue : RealValue);


        base.ProcEvent(revert);
    }
}