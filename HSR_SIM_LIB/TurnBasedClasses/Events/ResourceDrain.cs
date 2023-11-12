using System;
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
        return TargetUnit.Name + " res drain : " + Val + " " + ResType + "(by " + SourceUnit.Name + ")";
    }

    public override void ProcEvent(bool revert)
    {
        //Resource drain

        if (RealVal == null)
        {
            RealVal = Math.Min(TargetUnit.GetRes(ResType).ResVal, (double)Val);
            if (!CanSetToZero && RealVal >= TargetUnit.GetRes(ResType).ResVal) RealVal -= 1;
            //Game mechanics depend on Val instead
            Val = RealVal;
        }

        TargetUnit.GetRes(ResType).ResVal += (double)-(revert ? -RealVal : RealVal);
        base.ProcEvent(revert);
    }
}