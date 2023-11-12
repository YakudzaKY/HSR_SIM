using System;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//add to party resource
public class PartyResourceGain : Event
{
    public PartyResourceGain(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public Resource.ResourceType ResType { get; set; }

    public Team TargetTeam { get; set; }

    public override string GetDescription()
    {
        return $"Party res gain :  {Val:f} {ResType} by {TargetUnit?.Name ?? "system"}";
    }

    public override void ProcEvent(bool revert)
    {
        var tarTeam = TargetTeam ?? TargetUnit.ParentTeam;
        var CurrentResVal = tarTeam.GetRes(ResType).ResVal;
        if (ResType == Resource.ResourceType.SP)
            if (CurrentResVal + Val > Constant.MaxSp)
                Val = Constant.MaxSp - CurrentResVal;

        if (ResType == Resource.ResourceType.TP)
            if (Val + CurrentResVal > Constant.MaxTp)
                Val = Constant.MaxTp - CurrentResVal;

        tarTeam.GetRes(ResType).ResVal += (double)(revert ? -Val : Val);


        base.ProcEvent(revert);
    }
}