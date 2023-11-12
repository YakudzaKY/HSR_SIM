using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//drain party resource
public class PartyResourceDrain : Event
{
    public PartyResourceDrain(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public Resource.ResourceType ResType { get; set; }

    public Team TargetTeam { get; set; }

    public override string GetDescription()
    {
        return "Party res drain : " + Val + " " + ResType;
    }

    public override void ProcEvent(bool revert)
    {
        var tarTeam = TargetTeam ?? SourceUnit.ParentTeam;
        //SP or technical points
        tarTeam.GetRes(ResType).ResVal -= (double)(revert ? -Val : Val);
        base.ProcEvent(revert);
    }
}