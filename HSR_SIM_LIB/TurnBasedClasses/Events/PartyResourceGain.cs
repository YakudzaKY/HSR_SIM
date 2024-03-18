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
        return $"Party res gain :  {Value:f} {ResType} by {TargetUnit?.Name ?? "system"}";
    }

    public override void ProcEvent(bool revert)
    {
        var tarTeam = TargetTeam ?? TargetUnit.ParentTeam;
        if (!revert)
        {
            var currentResVal = tarTeam.GetRes(ResType).ResVal;
            if (ResType == Resource.ResourceType.SP)
                if (currentResVal + Value > Constant.MaxSp)
                    Value = Constant.MaxSp - currentResVal;

            if (ResType == Resource.ResourceType.TP)
                if (Value + currentResVal > Constant.MaxTp)
                    Value = Constant.MaxTp - currentResVal;
        }

        tarTeam.GetRes(ResType).ResVal += (double)(revert ? -Value : Value);


        base.ProcEvent(revert);
    }
}