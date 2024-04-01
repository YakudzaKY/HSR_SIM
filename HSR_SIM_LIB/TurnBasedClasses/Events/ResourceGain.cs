using System;
using HSR_SIM_LIB.Skills;
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
        return TargetUnit.Name + " res gain : " + Value + " " + ResType + "(by " + SourceUnit.Name + ")";
    }

    public override void ProcEvent(bool revert)
    {
        var res = TargetUnit.GetRes(ResType);
        //Resource drain
        RealValue ??= ResType switch
        {
            Resource.ResourceType.Toughness => Math.Min((double)Value, TargetUnit.Stats.MaxToughness - res.ResVal),
            Resource.ResourceType.HP => Math.Min(TargetUnit.MaxHp().Result - res.ResVal, (double)Value),
            Resource.ResourceType.Energy => Math.Min((double)Value, TargetUnit.Fighter.MaxEnergy - res.ResVal),
            _ => Value
        };
        res.ResVal += (double)(revert ? -RealValue : RealValue);
        if (!TriggersHandled)
        {
            if (RealValue != 0 && ResType == Resource.ResourceType.HP)
                TargetUnit.ResetCondition(Condition.ConditionCheckParam.Hp);
            TargetUnit.ResetCondition(Condition.ConditionCheckParam.Resource);
        }
        base.ProcEvent(revert);
    }
}