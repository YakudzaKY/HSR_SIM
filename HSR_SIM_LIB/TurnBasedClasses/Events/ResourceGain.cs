using System;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//drain some resource
public class ResourceGain : Event
{
    public ResourceGain(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public Resource.ResourceType ResType { get; set; }

    public override string GetDescription()
    {
        return TargetUnit.Name + " res gain : " + Val + " " + ResType + "(by " + SourceUnit.Name + ")";
    }

    public override void ProcEvent(bool revert)
    {
        var res = TargetUnit.GetRes(ResType);
        //Resource drain
        RealVal ??= ResType switch
        {
            Resource.ResourceType.Toughness => Math.Min((double)Val, TargetUnit.Stats.MaxToughness - res.ResVal),
            Resource.ResourceType.HP => Math.Min(TargetUnit.GetMaxHp(null) - res.ResVal, (double)Val),
            Resource.ResourceType.Energy => Math.Min((double)Val, TargetUnit.Stats.BaseMaxEnergy - res.ResVal),
            _ => Val
        };
        res.ResVal += (double)(revert ? -RealVal : RealVal);
        base.ProcEvent(revert);
    }
}